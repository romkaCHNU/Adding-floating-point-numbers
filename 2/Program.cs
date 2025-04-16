using System;

class Program
{
    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        float num1 = 5.25f;
        float num2 = -3.5f;

        string binaryNum1 = ConvertToBinary(num1, 8);
        Console.WriteLine($"Число 1: {num1} -> Бінарно: {binaryNum1}");
        Console.WriteLine($"Нормалізовано: {Normalize(num1)}\n");
        
        string binaryNum2 = ConvertToBinary(num2, 16);
        Console.WriteLine($"Число 2: {num2} -> Бінарно: {binaryNum2}");
        Console.WriteLine($"Нормалізовано: {Normalize(num2)}\n");

        string binarySum = AddFloatBinary(Convert8To16Bit(binaryNum1), binaryNum2);

        Console.WriteLine($"Результат додавання у бінарному вигляді: {binarySum}");

        float result = ConvertFromBinary(binarySum);
        Console.WriteLine($"Результат додавання: {result}");
        Console.WriteLine($"Нормалізовано: {Normalize(result)}\n");
    }

    static string ConvertToBinary(float num, int bits)
    {
        int sign = num < 0 ? 1 : 0; // Знак числа

        num = Math.Abs(num);
        int exponent = 0;

        // Робимо число нормалізованим (mantissa * 2^exp)
        while (num >= 2.0f)
        {
            num /= 2.0f;
            exponent++;
        }
        while (num < 1.0f)
        {
            num *= 2.0f;
            exponent--;
        }
        num -= 1.0f; // Відкидаємо неявну одиницю перед крапкою

        string mantissa = "";
        int mantissaBits = bits == 8 ? 4 : 10; 

        // Формуємо бітову мантису
        for (int i = 0; i < mantissaBits; i++)
        {
            num *= 2.0f;
            if (num >= 1.0f)
            {
                mantissa += "1";
                num -= 1.0f;
            }
            else
            {
                mantissa += "0";
            }
        }

        int expBias = bits == 8 ? 3 : 15; // Зсув експоненти (bias = 2^(n - 1) - 1)
        int biasedExp = exponent + expBias;
        int expBits = bits == 8 ? 3 : 5; 

        // Формуємо бінарну експоненту
        string exponentBinary = Convert.ToString(biasedExp, 2).PadLeft(expBits, '0');

        return sign.ToString() + exponentBinary + mantissa;
    }

    static string Convert8To16Bit(string binary)
    {
        int sign = int.Parse(binary.Substring(0, 1)); 
        int exp8 = Convert.ToInt32(binary.Substring(1, 3), 2);
        string mant8 = binary.Substring(4); 

        int exp16 = exp8 + (15 - 3); // Коригуємо експоненту під 16-бітну систему
        string exp16Str = Convert.ToString(exp16, 2).PadLeft(5, '0'); 

        string mant16 = mant8.PadRight(10, '0'); // Доповнюємо мантису нулями

        return sign + exp16Str + mant16;
    }

    static string AddFloatBinary(string bin1, string bin2)
    {
        int sign1 = bin1[0] - '0';
        int exp1 = Convert.ToInt32(bin1.Substring(1, 5), 2);
        string mant1 = "1" + bin1.Substring(6); // Додаємо приховану "1"

        int sign2 = bin2[0] - '0';
        int exp2 = Convert.ToInt32(bin2.Substring(1, 5), 2);
        string mant2 = "1" + bin2.Substring(6);

        int expDiff = exp1 - exp2;

        // Вирівнюємо експоненти
        if (expDiff > 0)
        {
            mant2 = ShiftRight(mant2, expDiff);
            exp2 = exp1;
        }
        else if (expDiff < 0)
        {
            mant1 = ShiftRight(mant1, -expDiff);
            exp1 = exp2;
        }

        string resultMant;
        bool resultNegative = false;

        // Якщо однакові знаки — додаємо мантиси, інакше виконуємо віднімання
        if (sign1 == sign2)
        {
            resultMant = AddBinary(mant1, mant2);
            resultNegative = sign1 == 1;
        }
        else
        {
            if (CompareBinary(mant1, mant2) >= 0)
            {
                resultMant = SubtractBinary(mant1, mant2);
                resultNegative = sign1 == 1;
            }
            else
            {
                resultMant = SubtractBinary(mant2, mant1);
                resultNegative = sign2 == 1;
            }
        }

        // Нормалізуємо результат
        if (resultMant.Length > 11)
        {
            resultMant = resultMant.Substring(1, 10); // зсуваємо і збільшуємо експоненту
            exp1 += 1;
        }
        else
        {
            int shift = resultMant.IndexOf('1');
            if (shift >= 0)
            {
                resultMant = resultMant.Substring(shift + 1).PadRight(10, '0'); // Зсуваємо до першої "1"
                exp1 -= shift;
            }
            else
            {
                resultMant = new string('0', 10); 
            }
        }

        string signBit = resultNegative ? "1" : "0";
        string expBits = Convert.ToString(exp1, 2).PadLeft(5, '0');

        return signBit + expBits + resultMant;
    }

    static string ShiftRight(string bin, int n) // Зсуваємо бітову стрічку праворуч
    {
        for (int i = 0; i < n; i++)
            bin = "0" + bin[..^1]; 

        return bin;
    }

    static string AddBinary(string a, string b)
    {
        string result = "";
        int carry = 0;
        for (int i = a.Length - 1; i >= 0; i--)
        {
            int sum = (a[i] - '0') + (b[i] - '0') + carry;
            result = (sum % 2) + result;
            carry = sum / 2;
        }
        return (carry > 0 ? "1" : "") + result;
    }

    static string SubtractBinary(string a, string b)
    {
        string result = "";
        int borrow = 0;
        for (int i = a.Length - 1; i >= 0; i--)
        {
            int bitA = a[i] - '0';
            int bitB = b[i] - '0' + borrow;

            if (bitA < bitB)
            {
                bitA += 2;
                borrow = 1;
            }
            else
            {
                borrow = 0;
            }
            result = (bitA - bitB) + result;
        }
        return result.TrimStart('0').PadLeft(11, '0');
    }

    static int CompareBinary(string a, string b)
    {
        for (int i = 0; i < a.Length; i++)
        {
            if (a[i] != b[i])
                return a[i] - b[i];
        }
        return 0;
    }

    static float ConvertFromBinary(string binary)
    {
        int sign = binary[0] == '1' ? -1 : 1;
        int exponent = Convert.ToInt32(binary.Substring(1, 5), 2) - 15;
        string mantissaBits = binary.Substring(6);

        double mantissa = 1.0;
        for (int i = 0; i < mantissaBits.Length; i++)
        {
            if (mantissaBits[i] == '1')
                mantissa += Math.Pow(2, -(i + 1));
        }

        return (float)(sign * mantissa * Math.Pow(2, exponent));
    }

    static string Normalize(float num)
    {
        string s = num < 0 ? "-" : "";
        num = Math.Abs(num);
        int exp = 0;
        while (num >= 2) { num /= 2; exp++; }
        while (num < 1 && num != 0) { num *= 2; exp--; }
        return $"{s}{num} * 2^{exp}";
    }
}
