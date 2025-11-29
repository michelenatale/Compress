



namespace michele.natale.Compresses;


/// <summary>
/// Represents the version of an archive format using a date-based schema
/// (Year.Month.Build.Revision).
/// </summary>
public class FcpVersion
{
  //The version number is fixed here.It can be changed on
  //request, provided that noticeable updates are introduced.

  /// <summary>
  /// The year component of the version
  /// </summary>
  public int Year { get; private set; } = 2025;

  /// <summary>
  /// The month component of the version.
  /// </summary>
  public byte Month { get; private set; } = 11;

  /// <summary>
  /// The build number component of the version.
  /// </summary>
  public byte Revision { get; private set; } = 1;

  /// <summary>
  /// The revision number component of the version.
  /// </summary>
  public byte Build { get; private set; } = 0;


  public FcpVersion()
  {
  }

  /// <summary>
  /// Create your own version
  /// </summary>
  /// <param name="year">Desired Year</param>
  /// <param name="month">Desired Month</param>
  /// <param name="build">Desired Build</param>
  /// <param name="revision">Desired Revision</param>
  public FcpVersion(int year, byte month, byte build, byte revision)
  {
    this.Year = year; this.Month = month;
    this.Build = build; this.Revision = revision;
  }

  /// <summary>
  /// Converts the version to its string representation in the format
  /// "YYYY.MM.BBB.RRR".
  /// </summary>
  /// <returns>
  /// A string representing the version, e.g. "2025.11.000.001".
  /// </returns>
  public override string ToString() => $"{this.Year}.{this.Month:D2}.{this.Build:D3}.{this.Revision:D3}";


  public long ToLong()
  {
    int[] y = [this.Year];
    var result = new byte[8];
    Buffer.BlockCopy(y, 0, result, 0, 4);
    result[5] = this.Month; result[6] = this.Build; result[7] = this.Revision;
    return BitConverter.ToInt64(result);
  }

  /// <summary>
  /// Gets the current version based on the current UTC year and month.  
  /// The build is set to 0 and the revision to 1.
  /// </summary>
  public static FcpVersion Current => new();

  /// <summary>
  /// Parses a version string in the format "YYYY.MM.BBB.RRR" into a
  /// <see cref="FcpVersion"/> instance.
  /// </summary>
  /// <param name="value">
  /// The version string to parse. Must contain four components separated by dots.
  /// </param>
  /// <returns>
  /// A <see cref="FcpVersion"/> instance representing the parsed version.
  /// </returns>
  /// <exception cref="FormatException">
  /// Thrown if the input string does not match the expected format.
  /// </exception>
  public static FcpVersion Parse(string value)
  {
    var parts = value.Split('.');
    if (parts.Length != 4) throw new FormatException("Invalid version format.");
    return new FcpVersion(
      int.Parse(parts[0]),
      byte.Parse(parts[1]),
      byte.Parse(parts[2]),
      byte.Parse(parts[3]));
  }

  public static FcpVersion Parse(long value)
  {
    var bytes = BitConverter.GetBytes(value);
    var y = BitConverter.ToInt32(bytes, 0);
    var m = bytes[5]; var b = bytes[6]; var r = bytes[7];
    return new(y, m, b, r);
  }

  /// <summary>
  /// Checks whether the given version is considered compatible with the current version.  
  /// By default, this method compares Year and Month components, ignoring Build and Revision.
  /// </summary>
  /// <param name="other">
  /// The version to check against the current version.
  /// </param>
  /// <returns>
  /// <c>true</c> if the version is considered compatible; otherwise <c>false</c>.
  /// </returns>
  public bool IsVersionOK(FcpVersion other) =>
    this.Year == other.Year && this.Month == other.Month;

  /// <summary>
  /// Checks whether the given version is considered compatible with the current version.  
  /// By default, this method compares Year and Month components, ignoring Build and Revision.
  /// </summary>
  /// <param name="other">
  /// The version to check against the current version.
  /// </param>
  /// <returns>
  /// <c>true</c> if the version is considered compatible; otherwise <c>false</c>.
  /// </returns>
  public static bool Is_Version_OK(FcpVersion other)
  {
    var current = Current;
    return current.Year == other.Year && current.Month == other.Month;
  }


  /// <summary>
  /// Checks whether the given version is considered compatible with the current version.  
  /// By default, this method compares Year and Month components, ignoring Build and Revision.
  /// </summary>
  /// <param name="other">
  /// The version to check against the current version.
  /// </param>
  /// <returns>
  /// <c>true</c> if the version is considered compatible; otherwise <c>false</c>.
  /// </returns>
  public static bool IsVersionOK(string other)
  {
    var current = Current;
    var fcpother = Parse(other);
    return current.Year == fcpother.Year && current.Month == fcpother.Month;
  }

  /// <summary>
  /// Checks whether two versions are considered compatible.  
  /// By default, this method compares Year and Month components,
  /// ignoring Build and Revision.
  /// </summary>
  /// <param name="left">The first version to compare.</param>
  /// <param name="right">The second version to compare.</param>
  /// <returns>
  /// <c>true</c> if both versions have the same year and month; otherwise <c>false</c>.
  /// </returns>
  public static bool IsVersionOK(FcpVersion left, FcpVersion right) =>
      left.Year == right.Year && left.Month == right.Month;

  public static bool IsVersionOK(FcpVersion left, string right)
  {
    var fcpother = Parse(right);
    return left.Year == fcpother.Year && left.Month == fcpother.Month;
  }


  public static bool IsVersionOK(FcpVersion left, long right)
  {
    var fcpother = Parse(right);
    return left.Year == fcpother.Year && left.Month == fcpother.Month;
  }
}