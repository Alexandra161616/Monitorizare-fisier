using System;
using System.Windows.Forms;
using System.IO;
using Microsoft.Win32.TaskScheduler;
using Microsoft.Win32;

namespace Proiect4._4
{
    public partial class Form1 : Form
    {
        //variabile pt path
        string rootPath;
        //path pentru task scheduler
        string rootPathTask;
        //variabila pt counting fisiere
        int counter = 0;
        //a creat variabila care va tine obiectu

        string keyName = @"HKEY_CURRENT_USER\Software\Problema4.4";
        //numele registrului unde va fi stocata info
        //in registrul software in subcheia UltimulDebug
        string valueName = "UltimulDebug";
        DateTime dateTime = DateTime.Now;
        //setezi valoarea in registru
        string ana;
        private FileSystemWatcher watcher;

        public Form1()
        {
            InitializeComponent();
            CreateTask();
            rootPath = SelectFolder();
            //combin path ul gasit de mine cu fisierul pe care il monitorizez
            //path ul gasit de mine e acela al proiectului, si eu adaug si numele folderului pt a specifica ce folder monitorizez
            rootPath = Path.Combine(rootPath, "ceva");

            //verific daca path ul e valid
            if (rootPath == null)
            {
                Environment.Exit(0);
            }

            //asta e pt watcher
            watcher = new FileSystemWatcher(); //a creat obiect
            watcher.Created += Watcher_Created;
            watcher.Filter = "*.txt";
            //selectezi ce tip de document verifici

            monitorizareFisier();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Visible = false;
            this.Opacity = 0;
            //sa nu se mai vada form ul
        }
        //test


        private string SelectFolder()
        {
            //aici ne trebuie calea absoluta a fisierului
            string fullPath = AppDomain.CurrentDomain.BaseDirectory;
            //returneaza path ul aplicatiei, cu tot cu bin\debug
            string parentDirectory = Directory.GetParent(fullPath).Parent.Parent.FullName;
            //folosim parent/parent de 2 ori pentru a elimina fisierele bin\debug, practic pt a ajunge la "nepot"
            return parentDirectory;

        }


        private void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Created)
            {
                counter++;
                string message = "A mai fost creat un file, iar acum avem " + counter + " files cu terminatia .txt";
                MessageBox.Show(message, "File Created", MessageBoxButtons.OK, MessageBoxIcon.Information);

                //setezi data si ora de la ultimul mesaj
                Registry.SetValue(keyName, valueName, dateTime.ToBinary(), RegistryValueKind.QWord);
                //setezi valoarea in registru

                TimeSpan diferenta = DateTime.Now.Subtract(GetDate(keyName, valueName));
                //aflii diferenta dintre data curenta si cea de la ultimul mesaj;
                if (diferenta.TotalHours >= 12)
                {
                    MessageBox.Show("Sun " + counter + " fisiere .txt");
                }
            }
            //e.ChangeType e setat la Created pentru a ne asigura ca event ul e special pt a file creation event
        }


        private void monitorizareFisier()
        {

            if (!Directory.Exists(rootPath))
            {//verifica daca exista un document in locatia specificata
                MessageBox.Show("Locatia e invalida", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }



            watcher.Path = rootPath;
            watcher.EnableRaisingEvents = true;
            //porneste watcher 

            //pui folderele intr o variabila de tip file
            var files = Directory.GetFiles(rootPath, "*.txt", SearchOption.AllDirectories);

            counter = files.Length;

            //verifica daca sunt mai mult de 2 fisiere txt in folder
            if (counter > 2)
            {
                string message = "Exista " + counter + " fisiere cu terminatia .txt";
                MessageBox.Show(message, "File number"); //asa ii pui textul si numele messagebox ului


                //setezi data si ora de la ultimul mesaj
                Registry.SetValue(keyName, valueName, dateTime.ToBinary(), RegistryValueKind.QWord);
                //setezi valoarea in registru

                TimeSpan diferenta = DateTime.Now.Subtract(GetDate(keyName, valueName));
                //afli diferenta dintre data curenta si cea de la ultimul mesaj;
                if (diferenta.TotalSeconds >= 1)
                {
                    MessageBox.Show("Sun " + counter + " fisiere .txt");
                }
            }
            else
            {
                


                using (RegistryKey explorerKey =
    Registry.CurrentUser.OpenSubKey("Software\\Problema4.4", writable: true))
                {//am deja curren user si nu mai trebuie toata adresa
                    if (explorerKey != null)
                    {
                        explorerKey.DeleteValue(valueName);
                    }
                }
            }

            }


        private void CreateTask()
        {
            rootPathTask = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            //pentru a lua adresa executabilului
            using (TaskService ts = new TaskService())
            {
                // Create a new task definition and assign properties
                TaskDefinition td = ts.NewTask();

                //pui nume task ului
                string taskName = "MonitorizareFisier";

                td.RegistrationInfo.Description = "Monitorizeaza un director si primim alerte daca acesta contine mai mult de 3 fisiere .txt si daca se mai fac schimbari in interiorul acestuia";

                // Creezi un trigger care va porni aplicatia la user logon
                LogonTrigger lt1 = new LogonTrigger();
                td.Triggers.Add(lt1);
                td.Principal.RunLevel = TaskRunLevel.Highest;

                // Creezi o actiune care va porni aplicatia de fiecare data cand e activat trigger ul
                td.Actions.Add(new ExecAction(rootPathTask, null));

                // inregistrezi task ul in root folder 
                //daca nu exista root folder creezi unul nou, iar daca exista il lasi asa
                TaskService.Instance.RootFolder.RegisterTaskDefinition(taskName, td).Run();

                //pt a da permisiune run visual studio as administrator!!!!!!!
            }

        }


        public static DateTime GetDate(string keyName, string valueName)
        {
            var rezultat = Registry.GetValue(keyName, valueName, null);
            if (rezultat != null && rezultat is long binaryValue)
            {
                return DateTime.FromBinary(binaryValue);
            }
            // Returneaza o valoare prestabilita de tip DateTime
            return DateTime.MinValue;
        }


    }
}
