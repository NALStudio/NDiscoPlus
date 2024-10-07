using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.PhilipsHue.Api.Models.RequestBuilders.Base;
public abstract partial class HueRequestBuilder
{
    // Keep inside HueRequestBuilder so that PropertyBuilder can access protected function parent.AddProperty()
    protected class PropertyBuilder : IDisposable
    {
        private readonly HueRequestBuilder parent;

        private readonly string propertyName;
        private readonly Dictionary<string, object> data;

        private bool propertyAddedToParent = false;

        public PropertyBuilder(HueRequestBuilder parent, string propertyName)
        {
            this.parent = parent;

            this.propertyName = propertyName;
            data = new();
        }

        public void AddProperty(string name, object value)
        {
            ThrowIfCommitted();

            data.Add(name, value);
        }

        public void AddPropertyIfNotNull<T>(string name, T? value) where T : class
        {
            ThrowIfCommitted();

            if (value is null)
                return;
            data.Add(name, value);
        }
        public void AddPropertyIfNotNull<T>(string name, T? value) where T : struct
        {
            ThrowIfCommitted();

            if (value.HasValue)
                data.Add(name, value.Value);
        }

        private void ThrowIfCommitted()
        {
            if (propertyAddedToParent)
                throw new InvalidOperationException("Cannot modify builder after committing changes.");
        }

        void IDisposable.Dispose() => CommitChanges();
        public void CommitChanges()
        {
            // only commit changes if changes haven't been committed yet
            // otherwise => no-op
            if (!propertyAddedToParent)
            {
                parent.AddProperty(propertyName, data);
                propertyAddedToParent = true;
            }
        }
    }
}
