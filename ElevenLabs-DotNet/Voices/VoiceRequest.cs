// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ElevenLabs.Voices
{
    public sealed class VoiceRequest : IDisposable
    {
        public VoiceRequest(string name, string samplePath, IReadOnlyDictionary<string, string> labels = null, string description = null)
            : this(name, [samplePath], labels, description)
        {
        }

        public VoiceRequest(string name, IEnumerable<string> samples, IReadOnlyDictionary<string, string> labels = null, string description = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentNullException.ThrowIfNull(samples);

            Name = name;
            Description = description;
            Labels = labels;
            Samples = samples.ToDictionary<string, string, Stream>(Path.GetFileName, File.OpenRead);
        }

        public VoiceRequest(string name, byte[] sample, IReadOnlyDictionary<string, string> labels = null, string description = null)
            : this(name, [sample], labels, description)
        {
        }

        public VoiceRequest(string name, IEnumerable<byte[]> samples, IReadOnlyDictionary<string, string> labels = null, string description = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentNullException.ThrowIfNull(samples);

            Name = name;
            Description = description;
            Labels = labels;
            var count = 0;
            Samples = samples.ToDictionary<byte[], string, Stream>(_ => $"file-{count++}", sample => new MemoryStream(sample));
        }

        public VoiceRequest(string name, Stream sample, IReadOnlyDictionary<string, string> labels = null, string description = null)
            : this(name, [sample], labels, description)
        {
        }

        public VoiceRequest(string name, IEnumerable<Stream> samples, IReadOnlyDictionary<string, string> labels = null, string description = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentNullException.ThrowIfNull(samples);

            Name = name;
            Description = description;
            Labels = labels;
            var count = 0;
            Samples = samples.ToDictionary(_ => $"file-{count++}", sample => sample);
        }

        ~VoiceRequest() => Dispose(false);

        public string Name { get; }

        public string Description { get; }

        public IReadOnlyDictionary<string, string> Labels { get; }

        public IReadOnlyDictionary<string, Stream> Samples { get; }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var (_, sample) in Samples)
                {
                    try
                    {
                        sample?.Close();
                        sample?.Dispose();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
