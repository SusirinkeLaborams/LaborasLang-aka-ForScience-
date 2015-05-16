use System;

auto num = 0;

entry auto main = void()
{
	int a;
	int b;
	int c;
	a = b = c = next();
	Console.WriteLine(a == b && b == c);
};

auto next = int()
{
	return num++;
};