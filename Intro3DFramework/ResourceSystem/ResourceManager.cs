using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intro3DFramework.ResourceSystem
{
    public static partial class ResourceManager
    {
        /// <summary>
        /// Internal list of all loaded resources.
        /// </summary>
        static private Dictionary<object, object> resourceDictionary = new Dictionary<object, object>();

        /// <summary>
        /// Loads or retrieves a resource with a given description.
        /// </summary>
        /// <remarks>
        /// If a resource with the same description has already been loaded, it will be looked up. Otherwise it will be loaded.
        /// </remarks>
        /// <typeparam name="ResourceType">Type of the resource to load. Needs to implement IResource.</typeparam>
        /// <typeparam name="DescriptionType">Corresponding descriptor type of the resource.</typeparam>
        /// <param name="description">Unique description of the resource.</param>
        /// <returns>A valid resource. Any errors will reported via an ResourceException.</returns>
        /// <exception cref="ResourceException">For any error during the creation/loading process a ResourceException may be thrown.</exception>
        /// <see cref="RemoveResource"/>
        static public void GetResource<ResourceType, DescriptionType>(out ResourceType resource, DescriptionType description)
            where ResourceType : BaseResource<ResourceType, DescriptionType>, new()
            where DescriptionType : IResourceDescription
        {
            object oldResource;
            if(!resourceDictionary.TryGetValue(description, out oldResource))
            {
                resource = new ResourceType();
                resource.DescriptionOnLoad = description;
                resource.Load(description);
                resourceDictionary.Add(description, resource);
            }
            else
            {
                resource = (ResourceType)oldResource;
            }
        }

        /// <summary>
        /// Unloads a resource by its description and removes from the internal list.
        /// </summary>
        /// <remarks>
        /// Attention, it also will dispose the resource. While it may be still referenced somewhere else, after this call it might be unusable!
        /// </remarks>
        /// <exception cref="ResourceException">If anything during the dispose goes wrong.</exception>
        /// <typeparam name="DescriptionType">Descriptor type of the resource.</typeparam>
        /// <param name="description">Unique description of the resource.</param>
        /// <returns>True if resource was found and disposed. False if the resource was not in the internal resource list.</returns>
        /// <see cref="GetResource"/>
        static public bool RemoveResource<DescriptionType>(ref DescriptionType description)  
            where DescriptionType : IResourceDescription
        {
            object resource;
            if (resourceDictionary.TryGetValue(description, out resource))
            {
                IDisposable disposable = resource as IDisposable;
                if(disposable != null)
                    disposable.Dispose();
                resourceDictionary.Remove(description);
                return true;
            }
            else
                return false;
        }
    }
}
