entry auto Main = void()
{
	int8 min_i8_literal    = -128;
	int8 max_i8_literal    = 127;
	uint8 min_u8_literal   = 0;
	uint8 max_u8_literal   = 255;
	char min_char_literal  = 0;
	char max_char_literal  = 65535;
	int16 min_i16_literal  = -32768;
	int16 max_i16_literal  = 32767;
	uint16 min_u16_literal = 0;
	uint16 max_u16_literal = 65535;
	int32 min_i32_literal  = -2147483648;
	int32 max_i32_literal  = 2147483647;
	uint32 min_u32_literal = 0;
	uint32 max_u32_literal = 4294967295;
	int64 min_i64_literal  = -9223372036854775808;
	int64 max_i64_literal  = 9223372036854775807;
	uint64 min_u64_literal = 0;
	uint64 max_u64_literal = 18446744073709551615;

	auto min_i8_builtin    = int8.MinValue;
	auto max_i8_builtin    = int8.MaxValue;
	auto min_u8_builtin    = uint8.MinValue;
	auto max_u8_builtin    = uint8.MaxValue;
	auto min_char_builtin  = char.MinValue;
	auto max_char_builtin  = char.MaxValue;
	auto min_i16_builtin   = int16.MinValue;
	auto max_i16_builtin   = int16.MaxValue;
	auto min_u16_builtin   = uint16.MinValue;
	auto max_u16_builtin   = uint16.MaxValue;
	auto min_i32_builtin   = int32.MinValue;
	auto max_i32_builtin   = int32.MaxValue;
	auto min_u32_builtin   = uint32.MinValue;
	auto max_u32_builtin   = uint32.MaxValue;
	auto min_i64_builtin   = int64.MinValue;
	auto max_i64_builtin   = int64.MaxValue;
	auto min_u64_builtin   = uint64.MinValue;
	auto max_u64_builtin   = uint64.MaxValue;

	auto output = string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, ",
                min_i8_literal, max_i8_literal, min_u8_literal, max_u8_literal, min_char_literal, max_char_literal, min_i16_literal, max_i16_literal,
				min_u16_literal, max_u16_literal, min_i32_literal, max_i32_literal, min_u32_literal, max_u32_literal, min_i64_literal, max_i64_literal, 
				min_u64_literal, max_u64_literal);
	
	output += string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, ",
                min_i8_builtin, max_i8_builtin, min_u8_builtin, max_u8_builtin, min_char_builtin, max_char_builtin, min_i16_builtin, max_i16_builtin,
				min_u16_builtin, max_u16_builtin, min_i32_builtin, max_i32_builtin, min_u32_builtin, max_u32_builtin, min_i64_builtin, max_i64_builtin, 
				min_u64_builtin, max_u64_builtin);
				
    output += string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, ",
                min_i8_literal == min_i8_builtin, max_i8_literal == max_i8_builtin, min_u8_literal == min_u8_builtin, max_u8_literal == max_u8_builtin, 
				min_char_literal == min_char_builtin, max_char_literal == max_char_builtin, min_i16_literal == min_i16_builtin, max_i16_literal == max_i16_builtin,
				min_u16_literal == min_u16_builtin, max_u16_literal == max_u16_builtin, min_i32_literal == min_i32_builtin, max_i32_literal == max_i32_builtin, 
				min_u32_literal == min_u32_builtin, max_u32_literal == max_u32_builtin, min_i64_literal == min_i64_builtin, max_i64_literal == max_i64_builtin, 
				min_u64_literal == min_u64_builtin, max_u64_literal == max_u64_builtin);

    output += string.Format("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}",
                min_i8_literal > 0, max_i8_literal > 0, min_u8_literal > 0, max_u8_literal > 0, min_char_literal > 0, max_char_literal > 0,
				min_i16_literal > 0, max_i16_literal > 0, min_u16_literal > 0, max_u16_literal > 0, min_i32_literal > 0, max_i32_literal > 0, 
				min_u32_literal > 0, max_u32_literal > 0, min_i64_literal > 0, max_i64_literal > 0, min_u64_literal > 0, max_u64_literal > 0);

	System.Console.WriteLine(output);
};