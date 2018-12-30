//MIT License
//Copyright(c) 2017 David Revoledo

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.
// Project Lead - David Revoledo davidrevoledo@d-genix.com

using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore.AutoHealthCheck
{
    /// <summary>
    ///     Basically is a Service collection wrapper for IAutoHealthCheckBuilder.
    /// </summary>
    internal class DefaultAutoHealthCheckBuilder : IAutoHealthCheckBuilder
    {
        private readonly IServiceCollection _service;

        public DefaultAutoHealthCheckBuilder(IServiceCollection services)
        {
            _service = services;
        }

        public IEnumerator<ServiceDescriptor> GetEnumerator()
        {
            return _service.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _service.GetEnumerator();
        }

        public void Add(ServiceDescriptor item)
        {
            _service.Add(item);
        }

        public void Clear()
        {
            _service.Clear();
        }

        public bool Contains(ServiceDescriptor item)
        {
            return _service.Contains(item);
        }

        public void CopyTo(ServiceDescriptor[] array, int arrayIndex)
        {
            _service.CopyTo(array, arrayIndex);
        }

        public bool Remove(ServiceDescriptor item)
        {
            return _service.Remove(item);
        }

        public int Count => _service.Count;

        public bool IsReadOnly => _service.IsReadOnly;

        public int IndexOf(ServiceDescriptor item)
        {
            return _service.IndexOf(item);
        }

        public void Insert(int index, ServiceDescriptor item)
        {
            _service.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _service.RemoveAt(index);
        }

        public ServiceDescriptor this[int index]
        {
            get => _service[index];
            set => _service[index] = value;
        }
    }
}
