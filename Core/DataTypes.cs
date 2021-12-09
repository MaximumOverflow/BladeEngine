using Silk.NET.OpenGL;

namespace BladeEngine.Core;

public enum DataType
{
	Unknown,
	Byte, SByte,
	Bool, Bool2, Bool3, Bool4,
	Int, Int2, Int3, Int4,
	UInt, UInt2, UInt3, UInt4,
	Long, Long2, Long3, Long4,
	ULong, ULong2, ULong3, ULong4,
	Short, Short2, Short3, Short4,
	UShort, UShort2, UShort3, Ushort4,
	Half, Half2, Half3, Half4,
	Float, Float2, Float3, Float4,
	Double, Double2, Double3, Double4,
	FloatMat4,
}

public static class DataTypes
{
	internal static DataType Get(UniformType type) => type switch
	{
		UniformType.Bool => DataType.Bool,
		UniformType.BoolVec2 => DataType.Bool2,
		UniformType.BoolVec3 => DataType.Bool3,
		UniformType.BoolVec4 => DataType.Bool4,
		UniformType.Int => DataType.Int,
		UniformType.IntVec2 => DataType.Int2,
		UniformType.IntVec3 => DataType.Int3,
		UniformType.IntVec4 => DataType.Int4,
		UniformType.UnsignedInt => DataType.UInt,
		UniformType.UnsignedIntVec2 => DataType.UInt2,
		UniformType.UnsignedIntVec3 => DataType.UInt3,
		UniformType.UnsignedIntVec4 => DataType.UInt4,
		UniformType.Float => DataType.Float,
		UniformType.FloatVec2 => DataType.Float2,
		UniformType.FloatVec3 => DataType.Float3,
		UniformType.FloatVec4 => DataType.Float4,
		UniformType.Double => DataType.Double,
		UniformType.DoubleVec2 => DataType.Double2,
		UniformType.DoubleVec3 => DataType.Double3,
		UniformType.DoubleVec4 => DataType.Double4,
		UniformType.FloatMat4 => DataType.FloatMat4,
		_ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
	};

	internal static DataType Get<T>()
	{
		if (typeof(T) == typeof(float)) return DataType.Float;
		if (typeof(T) == typeof(Vector2)) return DataType.Float2;
		if (typeof(T) == typeof(Vector3)) return DataType.Float3;
		if (typeof(T) == typeof(Vector4)) return DataType.Float4;
		if (typeof(T) == typeof(Matrix4x4)) return DataType.FloatMat4;
		return DataType.Unknown; //TODO Complete this list
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool Cast<TIn, TOut>(TIn value, out TOut? result)
	{
		if (typeof(TIn) != typeof(TOut))
		{
			result = default;
			return false;
		}

		result = Unsafe.As<TIn, TOut>(ref value);
		return true;
	}
}