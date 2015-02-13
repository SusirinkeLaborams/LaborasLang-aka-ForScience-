using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaborasLangCompiler.Parser
{
    struct Literal
    {
        public IConvertible Value { get; private set; }

        public Literal(IConvertible value) : this()
        {
            Value = value;
        }

        public static explicit operator bool(Literal _this)
        {
            return _this.Value.ToBoolean(CultureInfo.InvariantCulture);
        }

        public static explicit operator char(Literal _this)
        {
            return _this.Value.ToChar(CultureInfo.InvariantCulture);
        }

        public static explicit operator sbyte(Literal _this)
        {
            return _this.Value.ToSByte(CultureInfo.InvariantCulture);
        }

        public static explicit operator byte(Literal _this)
        {
            return _this.Value.ToByte(CultureInfo.InvariantCulture);
        }

        public static explicit operator short(Literal _this)
        {
            return _this.Value.ToInt16(CultureInfo.InvariantCulture);
        }

        public static explicit operator ushort(Literal _this)
        {
            return _this.Value.ToUInt16(CultureInfo.InvariantCulture);
        }

        public static explicit operator int(Literal _this)
        {
            return _this.Value.ToInt32(CultureInfo.InvariantCulture);
        }

        public static explicit operator uint(Literal _this)
        {
            return _this.Value.ToUInt32(CultureInfo.InvariantCulture);
        }

        public static explicit operator long(Literal _this)
        {
            return _this.Value.ToInt64(CultureInfo.InvariantCulture);
        }

        public static explicit operator ulong(Literal _this)
        {
            return _this.Value.ToUInt64(CultureInfo.InvariantCulture);
        }

        public static explicit operator float(Literal _this)
        {
            return _this.Value.ToSingle(CultureInfo.InvariantCulture);
        }

        public static explicit operator double(Literal _this)
        {
            return _this.Value.ToDouble(CultureInfo.InvariantCulture);
        }

        public static explicit operator decimal(Literal _this)
        {
            return _this.Value.ToDecimal(CultureInfo.InvariantCulture);
        }

        public static explicit operator string(Literal _this)
        {
            return _this.Value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
