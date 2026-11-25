#
# This file presents a serialization logic of x86-64 opcode map
#
# @local_environment: Python 3.12+
# @local_machine: atvlg
#
import json
import re
import pathlib

MODRM_PREFIXES = set("EGMNPQRUVWCDS")
# Intel decoder just rereads entries and returns 
# instruction mnemonic with parsed operands, so .x86 code must be shorten
# 
# instead of 
#   push $04000100
#   call far $000100f0
#   
# expected short and strict output like
#   PUSH 0x04000100     # object:offset 68 .. .. .. ..
#   CALLF 0x000100f0    # object:offset 9A .. .. .. ..

MNEMONIC_OVERRIDES = {
    "JMP-near": "JMP",
    "JMP-short": "JMP",
    "JMP-far": "JMPF",
    "CALLF": "CALLF",
    "RETN": "RET",
    "CBW/CWDE/CDQE": "CWDE",
    "CWD/CDQ/CQO": "CDQ",
    "IRET/D/Q": "IRETQ",
    "PUSHF/D/Q": "PUSHFQ",
    "POPF/D/Q": "POPFQ",
    "INS/INSB": "INS",
    "INS/INSW/INSD": "INS",
    "OUTS/OUTSB": "OUTS",
    "OUTS/OUTSW/OUTSD": "OUTS",
    "MOVS/B": "MOVS",
    "MOVS/W/D/Q": "MOVS",
    "CMPS/B": "CMPS",
    "CMPS/W/D": "CMPS",
    "STOS/B": "STOS",
    "STOS/W/D/Q": "STOS",
    "LODS/B": "LODS",
    "LODS/W/D/Q": "LODS",
    "SCAS/B": "SCAS",
    "SCAS/W/D/Q": "SCAS",
    "XLAT/XLATB": "XLATB",
    "FWAIT/WAIT": "WAIT",
}


def has_modrm(operands):
    return any(
        sub and sub[0] in MODRM_PREFIXES and not sub.startswith("ST")
        for op in operands
        for sub in op.split("/")
    )


def clean_mnemonic(mnem):
    if mnem.startswith("REX"):
        return "REX"
    if mnem in MNEMONIC_OVERRIDES:
        return MNEMONIC_OVERRIDES[mnem]
    if "/" in mnem and not mnem.startswith("Grp"):
        return mnem.split("/", 1)[0].strip()
    return mnem


def clean_operands(ops):
    keep_exact = {
        "rAX/r8", "rCX/r9", "rDX/r10", "rBX/r11",
        "rSP/r12", "rBP/r13", "rSI/r14", "rDI/r15",
        "AL/R8L", "CL/R9L", "DL/R10L", "BL/R11L",
        "AH/R12L", "CH/R13L", "DH/R14L", "BH/R15L",
    }
    res = []
    for op in map(str.strip, ops):
        if not op:
            continue
        if op in keep_exact:
            res.append(op)
        elif "/" in op:
            res.append(op.split("/", 1)[0].strip())
        else:
            res.append(op)
    return res


def pick_64bit_variant(raw_def):
    variants = []
    for part in raw_def.split("|"):
        part = part.strip()
        if not part:
            continue
        tags = re.findall(r"\(([^)]+)\)", part)
        clean = re.sub(r"\([^)]+\)", "", part).strip()
        variants.append((clean, tags))

    for clean, tags in variants:
        if "o64" in tags:
            return clean, tags

    legacy_64 = [
        (c, t) for c, t in variants
        if "i64" not in t and not any(x in t for x in ("v", "v1", "ev", "evo", "es", "xop"))
    ]
    if legacy_64:
        no_prefix = [x for x in legacy_64 if not any(p in x[1] for p in ("66", "F2", "F3"))]
        return no_prefix[0] if no_prefix else legacy_64[0]

    if variants:
        return "#UD", []

    return None, []


def parse_tables(input_path):
    tables, groups = {}, {}
    curr_type, curr_name = None, None

    with open(input_path, "r", encoding="utf-8") as f:
        for raw_line in f:
            line = raw_line.strip()
            if not line or line.startswith("#"):
                continue

            if line.startswith("Table:"):
                _, name = line.split(":", 1)
                curr_type, curr_name = "table", name.strip()
                tables[curr_name] = {}
            elif line.startswith("GrpTable:"):
                _, name = line.split(":", 1)
                curr_type, curr_name = "group", name.strip()
                groups[curr_name] = {}
            elif line == "EndTable":
                curr_type, curr_name = None, None
            elif ":" in line and curr_type:
                if line.startswith(("Referrer:", "AVXcode:", "XOPcode:")):
                    continue
                key, rest = line.split(":", 1)
                if "escape #" not in rest and "#" in rest:
                    rest, _ = rest.split("#", 1)
                target = tables[curr_name] if curr_type == "table" else groups[curr_name]
                target[key.strip().upper()] = rest.strip()

    return tables, groups


def split_mnemonic_operands(clean_text):
    tokens = clean_text.split(None, 1)
    mnem = tokens[0]
    ops = tokens[1].split(",") if len(tokens) > 1 else []
    return mnem, ops


def build_json(input_path, output_path):
    tables, groups = parse_tables(input_path)
    out_list = []

    target_tables = [
        ("", tables.get("one byte opcode", {})),
        ("0F", tables.get("2-byte opcode (0x0f)", {})),
    ]

    for prefix_hex, table in target_tables:
        for op_hex, raw_def in table.items():
            if not raw_def:
                continue

            full_opcode = f"{prefix_hex}{op_hex}"

            if raw_def.startswith("escape"):
                if full_opcode == "0F":
                    out_list.append({
                        "Opcode": "0F",
                        "Reg": None,
                        "Mnemonic": "0F",
                        "Operands": [],
                        "ModRM": False,
                        "IsPrefix": True,
                    })
                continue

            clean, _ = pick_64bit_variant(raw_def)
            if not clean:
                continue

            raw_mnem, raw_ops = split_mnemonic_operands(clean)
            ops = clean_operands(raw_ops)

            if raw_mnem in groups:
                out_list.append({
                    "Opcode": full_opcode,
                    "Reg": None,
                    "Mnemonic": raw_mnem,
                    "Operands": ops,
                    "ModRM": True,
                    "IsPrefix": False,
                })
                for reg_key, grp_def in groups[raw_mnem].items():
                    if not grp_def or not reg_key.isdigit():
                        continue
                    g_clean, _ = pick_64bit_variant(grp_def)
                    if not g_clean or g_clean == "#UD":
                        continue
                    g_raw_mnem, g_raw_ops = split_mnemonic_operands(g_clean)
                    g_ops = clean_operands(g_raw_ops) if g_raw_ops else ops
                    reg_num = int(reg_key)

                    out_list.append({
                        "Opcode": f"{full_opcode}/{reg_num}",
                        "Reg": reg_num,
                        "Mnemonic": clean_mnemonic(g_raw_mnem),
                        "Operands": g_ops,
                        "ModRM": True,
                        "IsPrefix": False,
                    })
            else:
                out_list.append({
                    "Opcode": full_opcode,
                    "Reg": None,
                    "Mnemonic": clean_mnemonic(raw_mnem),
                    "Operands": ops,
                    "ModRM": has_modrm(ops),
                    "IsPrefix": False,
                })

    with open(output_path, "w", encoding="utf-8") as f:
        json.dump(out_list, f, indent=2)


if __name__ == "__main__":
    script_domain_path = pathlib.Path(__file__).resolve().parent

    source_path = script_domain_path.joinpath(pathlib.Path("x86-opcode-map.txt"))
    result_path = script_domain_path.joinpath(pathlib.Path("x86_64_opcodes.json"))

    build_json(source_path, result_path)
