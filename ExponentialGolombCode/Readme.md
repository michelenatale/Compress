# Exponential Golomb Codes 

The 'ExpGolombCode' created here represents integers as bit patterns, whereby the shorter codes are adapted to the frequency of the numerical values. 

The EGCode is also calculated as prefix-free and unary by the algorithm created here, and thus implemented for efficient or effective compression. 

This is a very simple and effective procedure that is very easy for anyone (programmers, mathematicians etc.) to understand.   

```
public static byte[] ToEgc(ReadOnlySpan<byte> bytes)
{
  if (!EGC.Isready) EGC.StartEGC();

  var score = Assessment(bytes); 
  var dbytes = FromDict(score);
  var gc = ExpGolomnEncode(bytes, score);

  var result = new byte[gc.Length + dbytes.Length + 1];
  result[0] -= (byte)dbytes.Length;
  Array.Copy(gc, 0, result, 1, gc.Length);
  Array.Copy(dbytes, 0, result, gc.Length + 1, dbytes.Length);
  return result;
}
```
```
public static byte[] ToEgc(ReadOnlySpan<byte> bytes)
{
  if (!EGC.Isready) EGC.StartEGC();

  var score = Assessment(bytes); 
  var dbytes = FromDict(score);
  var gc = ExpGolomnEncode(bytes, score);

  ....
}
```
```
public static byte[] FromEgc(ReadOnlySpan<byte> bytes)
{
  if (!EGC.Isready) EGC.StartEGC();

  var fzc = bytes[1];
  var length = bytes.Length;
  var cb = (byte)(-bytes[0]); 
  var score = ToDict(bytes[(length - cb)..]);
  return ExpGolomnDecode(bytes[2..(length - cb)], score, fzc);
}
```

This is a very simple and effective procedure that is very easy for anyone (programmers, mathematicians etc.) to understand.   



