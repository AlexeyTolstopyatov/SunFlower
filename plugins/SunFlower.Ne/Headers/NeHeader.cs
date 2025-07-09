using System.Runtime.InteropServices;

namespace SunFlower.Ne.Headers;

public struct NeHeader
{ 
    public NeHeader() {}
    
    [MarshalAs(UnmanagedType.U2)] public ushort magic = 0; // "Магическое число"
    [MarshalAs(UnmanagedType.U1)] public byte ver = 0; // "Номер версии"
    [MarshalAs(UnmanagedType.U1)] public byte rev = 0; // "Номер ревизии"
    [MarshalAs(UnmanagedType.U2)] public ushort enttab = 0; // "Смещение таблицы входа"
    [MarshalAs(UnmanagedType.U2)] public ushort cbenttab = 0; // "Количество байт в таблице сущностей"
    [MarshalAs(UnmanagedType.U4)] public uint crc = 0; // "Контрольная сумма всего файла"
    [MarshalAs(UnmanagedType.U1)] public byte pflags = 0; // Тип программы. (архитектура, требования к загрузчику)
    [MarshalAs(UnmanagedType.U1)] public byte aflags = 0; // Тип приложения. (приложение/драйвер/библиотека)
    [MarshalAs(UnmanagedType.U2)] public ushort autodata = 0; // "Автоматический сегмент данных"
    [MarshalAs(UnmanagedType.U2)] public ushort heap = 0; // "Начальное выделение памяти для кучи"
    [MarshalAs(UnmanagedType.U2)] public ushort stack = 0; // "Начальное распределение стека"
    [MarshalAs(UnmanagedType.U4)] public uint csip = 0; // "Начальные установки CS:IP"
    [MarshalAs(UnmanagedType.U4)] public uint sssp = 0; // "Начальные установки SS:SP"
    [MarshalAs(UnmanagedType.U2)] public ushort cseg = 0; // "Количество сегментов в файле"
    [MarshalAs(UnmanagedType.U2)] public ushort cmod = 0; // "Записи в таблице модулей"
    [MarshalAs(UnmanagedType.U2)] public ushort cbnrestab = 0; // "Размер таблицы имен несуществующих сегментов"
    [MarshalAs(UnmanagedType.U2)] public ushort segtab = 0; // "Смещение таблицы сегментов"
    [MarshalAs(UnmanagedType.U2)] public ushort rsrctab = 0; // "Смещение таблицы ресурсов"
    [MarshalAs(UnmanagedType.U2)] public ushort restab = 0; // "Смещение таблицы имен несуществующих сегментов"
    [MarshalAs(UnmanagedType.U2)] public ushort modtab = 0; // "Смещение таблицы ссылок на модули"
    [MarshalAs(UnmanagedType.U2)] public ushort imptab = 0; // "Смещение таблицы импортированных имен"
    [MarshalAs(UnmanagedType.U4)] public uint nrestab = 0; // "Смещение таблицы несуществующих имен"
    [MarshalAs(UnmanagedType.U2)] public ushort cmovent = 0; // "Количество перемещаемых записей"
    [MarshalAs(UnmanagedType.U2)] public ushort align = 0; // "Сдвиг смещения сегмента"
    [MarshalAs(UnmanagedType.U2)] public ushort cres = 0; // "Сдвиг смещения сегмента"
    [MarshalAs(UnmanagedType.U1)] public byte os = 0; // "Целевая операционная система"
    [MarshalAs(UnmanagedType.U1)] public byte flagsothers = 0; // "Другие флаги .EXE"
    [MarshalAs(UnmanagedType.U2)] public ushort pretthunks = 0; // "Смещение возвратных фреймов"
    [MarshalAs(UnmanagedType.U2)] public ushort psegrefbytes = 0; // "Смещение сегментных ссылочных байтов"
    [MarshalAs(UnmanagedType.U2)] public ushort swaparea = 0; // "Минимальный размер области свопа кода"
    [MarshalAs(UnmanagedType.U1)] public byte minor = 0; // "Ожидаемая версия Windows [min.maj]"
    [MarshalAs(UnmanagedType.U1)] public byte major = 0;
}