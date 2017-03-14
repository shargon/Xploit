namespace XPloit.Core.Mongo
{
    public class RemovableValue<T>
    {
        public T Value { get; set; }
        public bool Remove { get; set; }

        public RemovableValue(T value) { Value = value; }
    }
}