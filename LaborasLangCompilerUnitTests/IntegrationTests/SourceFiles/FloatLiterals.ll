use System;

entry auto Program = void()
{
	auto float1 = 0F;
	auto float2 = 1f;
	auto float3 = 1.5f;
	auto float4 = 1.5F;

	Console.WriteLine("{1}{0}{2}{0}{3}{0}{4}", Environment.NewLine, float1, float2, float3, float4);
};