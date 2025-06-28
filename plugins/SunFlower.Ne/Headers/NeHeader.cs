using System.Runtime.InteropServices;

namespace SunFlower.Ne.Headers;

public struct NeHeader
{ 
    public NeHeader() {}
    
    [MarshalAs(UnmanagedType.U2)] public UInt16 magic = 0; // "Магическое число"
    [MarshalAs(UnmanagedType.U1)] public Byte ver = 0; // "Номер версии"
    [MarshalAs(UnmanagedType.U1)] public Byte rev = 0; // "Номер ревизии"
    [MarshalAs(UnmanagedType.U2)] public UInt16 enttab = 0; // "Смещение таблицы входа"
    [MarshalAs(UnmanagedType.U2)] public UInt16 cbenttab = 0; // "Количество байт в таблице сущностей"
    [MarshalAs(UnmanagedType.U4)] public UInt32 crc = 0; // "Контрольная сумма всего файла"
    [MarshalAs(UnmanagedType.U1)] public Byte pflags = 0; // Тип программы. (архитектура, требования к загрузчику)
    [MarshalAs(UnmanagedType.U1)] public Byte aflags = 0; // Тип приложения. (приложение/драйвер/библиотека)
    [MarshalAs(UnmanagedType.U2)] public UInt16 autodata = 0; // "Автоматический сегмент данных"
    [MarshalAs(UnmanagedType.U2)] public UInt16 heap = 0; // "Начальное выделение памяти для кучи"
    [MarshalAs(UnmanagedType.U2)] public UInt16 stack = 0; // "Начальное распределение стека"
    [MarshalAs(UnmanagedType.U4)] public UInt32 csip = 0; // "Начальные установки CS:IP"
    [MarshalAs(UnmanagedType.U4)] public UInt32 sssp = 0; // "Начальные установки SS:SP"
    [MarshalAs(UnmanagedType.U2)] public UInt16 cseg = 0; // "Количество сегментов в файле"
    [MarshalAs(UnmanagedType.U2)] public UInt16 cmod = 0; // "Записи в таблице модулей"
    [MarshalAs(UnmanagedType.U2)] public UInt16 cbnrestab = 0; // "Размер таблицы имен несуществующих сегментов"
    [MarshalAs(UnmanagedType.U2)] public UInt16 segtab = 0; // "Смещение таблицы сегментов"
    [MarshalAs(UnmanagedType.U2)] public UInt16 rsrctab = 0; // "Смещение таблицы ресурсов"
    [MarshalAs(UnmanagedType.U2)] public UInt16 restab = 0; // "Смещение таблицы имен несуществующих сегментов"
    [MarshalAs(UnmanagedType.U2)] public UInt16 modtab = 0; // "Смещение таблицы ссылок на модули"
    [MarshalAs(UnmanagedType.U2)] public UInt16 imptab = 0; // "Смещение таблицы импортированных имен"
    [MarshalAs(UnmanagedType.U4)] public UInt32 nrestab = 0; // "Смещение таблицы несуществующих имен"
    [MarshalAs(UnmanagedType.U2)] public UInt16 cmovent = 0; // "Количество перемещаемых записей"
    [MarshalAs(UnmanagedType.U2)] public UInt16 align = 0; // "Сдвиг смещения сегмента"
    [MarshalAs(UnmanagedType.U2)] public UInt16 cres = 0; // "Сдвиг смещения сегмента"
    [MarshalAs(UnmanagedType.U1)] public Byte os = 0; // "Целевая операционная система"
    [MarshalAs(UnmanagedType.U1)] public Byte flagsothers = 0; // "Другие флаги .EXE"
    [MarshalAs(UnmanagedType.U2)] public UInt16 pretthunks = 0; // "Смещение возвратных фреймов"
    [MarshalAs(UnmanagedType.U2)] public UInt16 psegrefbytes = 0; // "Смещение сегментных ссылочных байтов"
    [MarshalAs(UnmanagedType.U2)] public UInt16 swaparea = 0; // "Минимальный размер области свопа кода"
    [MarshalAs(UnmanagedType.U1)] public Byte minor = 0; // "Ожидаемая версия Windows [min.maj]"
    [MarshalAs(UnmanagedType.U1)] public Byte major = 0;
}