using XPloit.Core;
using XPloit.Core.Helpers;

namespace XPloit.Modules.Encoders.String
{
    public class JsonEncoder : Encoder
    {
        public override string Author { get { return "Fernando Díaz Toledano"; } }
        public override string Description { get { return "Encode to json string"; } }
        public override string Name { get { return "JsonEncoder"; } }
        public override string Path { get { return "Encoders/String"; } }

        /// <summary>
        /// Constructor
        /// </summary>
        public JsonEncoder() { }

        /// <summary>
        /// Encode
        /// </summary>
        /// <param name="input">Input</param>
        /// <returns>Return object</returns>
        public override object Run(Payload payload)
        {
            if (payload == null) return null;
            return JsonHelper.Serialize(payload);
        }
    }
}