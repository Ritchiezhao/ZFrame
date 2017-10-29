
#if UNITY_EDITOR
using UnityEngine;
#endif

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

#if UNITY_EDITOR
public static class UnityExtensions
{
	public static void ToRange(this Vector2 v, float minX, float maxX, float minY, float maxY)
	{
		v.x = Mathf.Min(maxX, v.x);
		v.x = Mathf.Max(minX, v.x);
		v.y = Mathf.Min(maxY, v.y);
		v.y = Mathf.Max(minY, v.y);
	}

	public static Vector2 GetInRange(this Vector2 v, float minX, float maxX, float minY, float maxY)
	{
		Vector2 ret = new Vector2();
		ret.x = Mathf.Min(maxX, v.x);
		ret.x = Mathf.Max(minX, v.x);
		ret.y = Mathf.Min(maxY, v.y);
		ret.y = Mathf.Max(minY, v.y);
		return v;
	}

}
#endif

public class SerializeUtil
{
	public static void WriteString(BinaryWriter bw, string str)
	{
		byte[] utf8 = Encoding.UTF8.GetBytes(str);
		bw.Write(utf8.Length);
		bw.Write(utf8);
	}
}


/// <summary>
/// Code generator.
/// </summary>
public class CodeGenerator
{
    int indentCount = 0;
    StringWriter writer;

    public CodeGenerator(StringWriter sw)
    {
        writer = sw;
    }

    public void AddIndent()
    {
        indentCount += 1;
    }

    public void AddIndent(string newLine)
    {
        Line(newLine);
        indentCount += 1;
    }

    public void RemIndent()
    {
        indentCount -= 1;
    }

    public void RemIndent(string newLine)
    {
        indentCount -= 1;
        Line(newLine);
    }

    public void SetIndent(int i)
    {
        indentCount = i;
    }

    public void Line()
    {
        writer.WriteLine();
    }


    void WriteIndent()
    {
        for (int i = 0; i < indentCount; ++i) {
            writer.Write("    ");
        }
    }

    public void Line(string line)
    {
        WriteIndent();
        writer.WriteLine(line);
    }

    public void Line(string format, params object[] args)
    {
        WriteIndent();
        writer.WriteLine(format, args);
    }

    public void Append(string str)
    {
        writer.Write(str);
    }

    public void Append(string format, params object[] args)
    {
        writer.Write(format, args);
    }
}
