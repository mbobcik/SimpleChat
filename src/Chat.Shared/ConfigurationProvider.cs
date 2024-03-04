using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Chat.Shared
{
    public class ConfigurationProvider<TConfigType> where TConfigType : ConfigurationBase, new()
    {
        private string Path;
        private FileSystemWatcher watcher;
        private Mutex mutex;

        public TConfigType Configuration { get; private set; }

        public ConfigurationProvider(string path)
        {
            mutex = new Mutex(false, $"{typeof(ConfigurationProvider<TConfigType>)}-{typeof(TConfigType)}");
            this.Path = path;
            mutex.WaitOne();
            
            if (!File.Exists(path))
            {
                RecreateConfigFile(path);
            }
            else
            {
                try
                {
                    LoadConfigFromFile(path);
                }
                catch
                {
                    File.Delete(path);
                    this.RecreateConfigFile(path);
                }
            }

            this.watcher = new FileSystemWatcher();
            watcher.Path = new FileInfo(this.Path).DirectoryName;
            watcher.Filter = path;
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Changed += this.OnChangedFile;
            watcher.Deleted += this.OnDeleted;
            
            mutex.ReleaseMutex();
        }

        private void LoadConfigFromFile(string path)
        {
            var jsonString = File.ReadAllText(path);
            try
            {
                this.Configuration = JsonSerializer.Deserialize<TConfigType>(jsonString);
                if (this.Configuration == null)
                {
                    this.RecreateConfigFile(path);
                }
            }
            catch
            {
                this.RecreateConfigFile(path);
            }
        }

        private void RecreateConfigFile(string path)
        {
            this.Configuration = new TConfigType();
            var jsonString = JsonSerializer.Serialize(Configuration, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, jsonString);
            
        }

        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            mutex.WaitOne();
            if (!File.Exists(this.Path))
            {
                this.RecreateConfigFile(this.Path);
            }
            mutex.ReleaseMutex();
        }

        private void OnChangedFile(object sender, FileSystemEventArgs e)
        {
            mutex.WaitOne();
            this.LoadConfigFromFile(this.Path);
            mutex.ReleaseMutex();
        }
    }
}
