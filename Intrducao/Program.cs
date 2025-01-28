using System;
using System.Diagnostics;
using System.IO;

namespace Biometria
{
    class Program
    {
        static string TempPath = @"C:\SYSPDV\TEMP";
        static string LogPath = Path.Combine(TempPath, "Log_Biometria.txt");
        static string SysPdvCadPath = @"C:\SYSPDV\SYSPDV_CAD.FDB";
        static string SysPdvMovPath = @"C:\SYSPDV\SYSPDV_MOV.FDB";
        static string FirebirdPath = @"C:\Program Files (x86)\Firebird\Firebird_2_5\bin";
        static string IscUser = "SYSDBA";
        static string IscPassword = "masterkey";

        static void Main(string[] args)
        {
            SetupPaths();
            VerifyPaths();
            Menu();
        }

        static void SetupPaths()
        {
            if (!Directory.Exists(TempPath))
            {
                Directory.CreateDirectory(TempPath);
                File.SetAttributes(TempPath, FileAttributes.Hidden);
            }

            if (Directory.Exists(@"C:\Program Files\Firebird\Firebird_2_5\bin"))
            {
                FirebirdPath = @"C:\Program Files\Firebird\Firebird_2_5\bin";
            }
        }

        static void VerifyPaths()
        {
            if (!File.Exists(SysPdvCadPath))
            {
                Console.WriteLine("\n[ERRO] Banco SYSPDV_CAD não encontrado!");
                Environment.Exit(0);
            }

            if (!File.Exists(SysPdvMovPath))
            {
                Console.WriteLine("\n[ERRO] Banco SYSPDV_MOV não encontrado!");
                Environment.Exit(0);
            }
        }

        static void Menu()
        {
            while (true)
            {
                Console.WriteLine("==================================");
                Console.WriteLine("Escolha a Opção da Biometria!");
                Console.WriteLine("1 - Desativar");
                Console.WriteLine("2 - Restaurar");
                Console.WriteLine("0 - Sair");
                Console.WriteLine("==================================");

                Console.Write("Digite a opção: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        DesativarBiometria();
                        break;
                    case "2":
                        RestaurarBiometria();
                        break;
                    case "0":
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("Opção inválida. Tente novamente.");
                        break;
                }
            }
        }

        static void DesativarBiometria()
        {
            Console.WriteLine("\nDesativando Biometria...");
            KillProcess("SYSPDV_PDV.EXE");

            string desativarCad = Path.Combine(TempPath, "Desativar_cad.sql");
            string desativarMov = Path.Combine(TempPath, "Desativar_mov.sql");

            File.WriteAllText(desativarCad, @"
SET HEADING OFF;
OUTPUT '" + Path.Combine(TempPath, "Locdigsen_Original.txt") + @"';
SELECT COALESCE(LOCDIGSEN, 'T') FROM CONFIGPDV;
UPDATE CONFIGPDV SET LOCDIGSEN = 'T';
EXIT;
");

            File.WriteAllText(desativarMov, @"
SET HEADING OFF;
OUTPUT '" + Path.Combine(TempPath, "Cxalocdigsen_Original.txt") + @"';
SELECT COALESCE(CXALOCDIGSEN, '0') FROM CAIXA;
UPDATE CAIXA SET CXALOCDIGSEN = '0';
EXIT;
");

            ExecuteSQL(desativarCad, SysPdvCadPath);
            ExecuteSQL(desativarMov, SysPdvMovPath);

            Console.WriteLine("\n[SUCESSO] Biometria Desativada!");
            RestartApp();
        }

        static void RestaurarBiometria()
        {
            Console.WriteLine("\nRestaurando Biometria...");
            KillProcess("SYSPDV_PDV.EXE");

            string restaurarCad = Path.Combine(TempPath, "Restaurar_cad.sql");
            string restaurarMov = Path.Combine(TempPath, "Restaurar_mov.sql");

            string locdigsenOriginal = File.Exists(Path.Combine(TempPath, "Locdigsen_Original.txt"))
                ? File.ReadAllText(Path.Combine(TempPath, "Locdigsen_Original.txt")).Trim()
                : "B";

            string cxalocdigsenOriginal = File.Exists(Path.Combine(TempPath, "Cxalocdigsen_Original.txt"))
                ? File.ReadAllText(Path.Combine(TempPath, "Cxalocdigsen_Original.txt")).Trim()
                : "1";

            File.WriteAllText(restaurarCad, $@"
UPDATE CONFIGPDV SET LOCDIGSEN = '{locdigsenOriginal}';
EXIT;
");

            File.WriteAllText(restaurarMov, $@"
UPDATE CAIXA SET CXALOCDIGSEN = '{cxalocdigsenOriginal}';
EXIT;
");

            ExecuteSQL(restaurarCad, SysPdvCadPath);
            ExecuteSQL(restaurarMov, SysPdvMovPath);

            Console.WriteLine("\n[SUCESSO] Biometria Restaurada!");
            RestartApp();
        }

        static void KillProcess(string processName)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "taskkill",
                    Arguments = $"/F /IM {processName}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }).WaitForExit();
            }
            catch
            {
                Console.WriteLine($"Erro ao finalizar o processo: {processName}");
            }
        }

        static void ExecuteSQL(string sqlFile, string databasePath)
        {
            try
            {
                Process process = Process.Start(new ProcessStartInfo
                {
                    FileName = Path.Combine(FirebirdPath, "isql.exe"),
                    Arguments = $"-USER {IscUser} -PASSWORD {IscPassword} \"{databasePath}\" -i \"{sqlFile}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });

                process.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao executar SQL: {ex.Message}");
            }
        }

        static void RestartApp()
        {
            Process.Start(@"C:\SYSPDV\SYSPDV_PDV.EXE");
        }
    }
}
