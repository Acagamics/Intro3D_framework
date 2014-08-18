using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Intro3DFramework.Rendering
{
    public class Texture2D : ResourceSystem.BaseResource<Texture2D, Texture2D.LoadDescription>
    {
        public uint Width { get; private set; }
        public uint Height { get; private set; }


        public struct LoadDescription : ResourceSystem.IResourceDescription
        {
            public static implicit operator LoadDescription(string filename)
            {
                return new LoadDescription(filename);
            }

            public LoadDescription(string filename, bool generateMipMaps = true)
            {
                this.filename = filename;
                this.generateMipMaps = generateMipMaps;
            }

            public bool Equals(ResourceSystem.IResourceDescription other)
            {
                LoadDescription? otherDesc = other as LoadDescription?;
                return otherDesc.HasValue &&
                       filename == otherDesc.Value.filename &&
                       generateMipMaps == otherDesc.Value.generateMipMaps;
            }

            public override int GetHashCode()
            {
                return filename.GetHashCode() * (generateMipMaps ? 2 : 1);
            }

            public string filename;
            public bool generateMipMaps;
        }

        /// <summary>
        /// Empty constructor, only for creation from file via ResourceManager
        /// </summary>
        public Texture2D()
        {
            throw new NotImplementedException();
        }


        internal override void Load(Texture2D.LoadDescription description)
        {
            throw new NotImplementedException();
        }



        #region Disposing

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion


    }
}
