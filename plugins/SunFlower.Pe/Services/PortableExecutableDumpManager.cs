using SunFlower.Pe.Headers;

namespace SunFlower.Pe.Services;

public class PortableExecutableDumpManager(string path) : IManager
{
    public MzHeader Dos2Header { get; set; }
    public PeFileHeader FileHeader { get; set; }
    public PeOptionalHeader32 OptionalHeader32 { get; set; }
    public PeOptionalHeader OptionalHeader { get; set; }
    public PeDirectory[] PeDirectories { get; set; } = [];
    public PeSection[] PeSections { get; set; } = [];
    public PeImageExportDirectory ExportDirectory { get; set; }
    
    public void Initialize()
    {
        Task.Run(() =>
        {
            FileStream stream = new(path, FileMode.Open, FileAccess.Read);
            BinaryReader reader = new(stream);
            
            
        });
    }
    
    private void FindDirectoriesCharacteristics()
    {
        String[] names =
        [
            "image_export_directory".ToUpper(),
            "image_import_directory".ToUpper(),
            "image_resource_directory".ToUpper(),
            "image_exception_directory".ToUpper(),
            "image_certification_directory".ToUpper(),
            "image_baserelocs_directory".ToUpper(),
            "image_debug_directory".ToUpper(),
            "image_architecture_directory".ToUpper(),
            "image_globalptr_directory".ToUpper(),
            "image_tls_directory".ToUpper(),
            "image_loadconfig_directory".ToUpper(),
            "image_boundimport_directory".ToUpper(),
            "image_iat_directory".ToUpper(),
            "image_delay_import_descriptors_directory".ToUpper(),
            "image_com_directory".ToUpper(),
            "image_reserved_directory".ToUpper()
        ];
    }
}