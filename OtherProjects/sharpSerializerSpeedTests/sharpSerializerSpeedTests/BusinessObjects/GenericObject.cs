using System;
namespace HelloWorldApp.BusinessObjects
{
    [Serializable]
    public class GenericObject<T>
    {
        public GenericObject()
        {
        }

        public GenericObject(T data)
        {
            Data = data;
        }

        public T Data { get; set; }
    }
}