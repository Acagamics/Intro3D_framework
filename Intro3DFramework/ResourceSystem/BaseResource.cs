using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intro3DFramework.ResourceSystem
{
    /// <summary>
    /// Base interface for all resources that can be handled by the ResourceManager.
    /// Additionally all resources need a parameterless constructor.
    /// </summary>
    /// <typeparam name="ResourceType">Type of the resource. Use the implementing class for this.</typeparam>
    /// <typeparam name="Descriptor">A comparable descriptor resource creation.</typeparam>
    public abstract class BaseResource<ResourceType, DescriptionType> : IDisposable
        where ResourceType : BaseResource<ResourceType, DescriptionType>, new()
        where DescriptionType : IResourceDescription
    {
        /// <summary>
        /// Description that was used for the first GetResource call.
        /// </summary>
        /// <remarks>
        /// ResourceManager sets this before calling the load method.
        /// </remarks>
        public DescriptionType DescriptionOnLoad
        {
            get { return descriptionOnLoad; }
            internal set { descriptionOnLoad = value; }
        }

        /// <see cref="DescriptionOnLoad"/>
        private DescriptionType descriptionOnLoad;

        /// <summary>
        /// Loads the resource using the given description.
        /// </summary>
        /// <exception cref="ResourceException">For any error a ResourceException may be thrown.</exception>
        /// <param name="description">Description defining different options to create the resource</param>
        internal abstract void Load(DescriptionType description);

        /// <summary>
        /// Loads or retrieves a resource with a given description.
        /// </summary>
        /// <remarks>
        /// It is basically a convenience wrapper for ResourceManager.GetResource.
        /// If a resource with the same description has already been loaded, it will be looked up. Otherwise it will be loaded.
        /// </remarks>
        /// <param name="description">Unique description of the resource.</param>
        /// <returns>A valid resource. Any errors will reported via an ResourceException.</returns>
        /// <exception cref="ResourceException">For any error during the creation/loading process a ResourceException may be thrown.</exception>
        /// <see cref="ResourceManager.GetResource"/>
        static public ResourceType GetResource(DescriptionType description)
        {
            ResourceType resource;
            ResourceManager.GetResource<ResourceType, DescriptionType>(out resource, description);
            return resource;
        }

        /// <summary>
        /// Removes this resource.
        /// </summary>
        /// <remarks>
        /// Convenience function for calling ResourceManager.RemoveResource
        /// </remarks>
        /// <see cref="ResourceManager.RemoveResource"/>
        public void RemoveResource()
        {
            ResourceManager.RemoveResource<DescriptionType>(ref descriptionOnLoad);
        }

        /// <summary>
        /// Most resources handle data that must be freed on destruction. Therefore they need to overwrite the dispose function.
        /// </summary>
        abstract public void Dispose();
    }

    /// <summary>
    /// Base interface for identifying all resource descriptions.
    /// </summary>
    public interface IResourceDescription
    {
    }
}