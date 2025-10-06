using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Game.Scripts.Infrastructure.Data.Persistence
{
    public static class JsonSaver
    {
        public static async UniTask SaveAsync<T>(T obj, string path, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
            
            string dir = Path.GetDirectoryName(path);
            
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            JsonSerializerSettings settings = new()
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto, 
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            };

            string json = JsonConvert.SerializeObject(obj, settings);

            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
            using (var sw = new StreamWriter(fs, Encoding.UTF8))
            {
                ct.ThrowIfCancellationRequested();
                await sw.WriteAsync(json).ConfigureAwait(false);
                await sw.FlushAsync().ConfigureAwait(false);
            }
        }

        public static async UniTask<T> LoadAsync<T>(string path, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
            
            if (!File.Exists(path)) throw new FileNotFoundException("File not found", path);

            string json;
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true))
            using (var sr = new StreamReader(fs, Encoding.UTF8))
            {
                ct.ThrowIfCancellationRequested();
                json = await sr.ReadToEndAsync().ConfigureAwait(false);
            }

            ct.ThrowIfCancellationRequested();

            JsonSerializerSettings settings = new()
            {
                TypeNameHandling = TypeNameHandling.Auto,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            };

            T obj = JsonConvert.DeserializeObject<T>(json, settings);
            return obj;
        }
    }
}
