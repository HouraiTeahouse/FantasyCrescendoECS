using System;
using Unity.Assertions;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Core;
using Unity.Entities.Serialization;

namespace HouraiTeahouse.FantasyCrescendo.Matches {

/// <summary>
/// A writer for replay files.
/// </summary>
public sealed class ReplayWriter : IDisposable {

  // Magic Header for the file type: "ddDa"
  // The first four notes of Megalovania.
  public static readonly byte[] kMagicHeader = new byte[] { 0x64, 0x64, 0x44, 0x61 };
  readonly BinaryWriter _writer;

  /// <summary>
  /// Creatse a ReplayWriter.
  /// </summary>
  /// <param name="writer">A BinaryWriter implementation, takes ownership of it.</param>
  public ReplayWriter(BinaryWriter writer) {
    _writer = writer;
  }

  /// <summary>
  /// Writes a 64-bit checksum to the file.
  /// </summary>
  /// <param name="checksum">a 64-bit checksum to write.</param>
  public void WriteChecksum(ulong checksum) => _writer.Write(checksum);

  /// <summary>
  /// Writes a frame of input to the file.
  /// </summary>
  /// <param name="inputs">a frame of input to write.</param>
  public unsafe void WriteInputs(ReadOnlySpan<PlayerInput> inputs) {
    Assert.IsTrue(inputs.Length == MatchConfig.kMaxSupportedPlayers);
    fixed (PlayerInput* inputPtr = inputs) {
      var ptr = (byte*)inputPtr;
      int size = MatchConfig.kMaxSupportedPlayers * UnsafeUtility.SizeOf<PlayerInput>();
      _writer.Write(XXHash.Hash64(ptr, size));
      _writer.WriteBytes(ptr, size);
    }
  }

  /// <summary>
  /// Disposes of the ReplayWriter. Also disposes of the underlying
  /// BinaryWriter.
  /// </summary>
  public void Dispose() => _writer.Dispose();

}

/// <summary>
/// A reader for replay files.
/// </summary>
public sealed class ReplayReader : IDisposable {

  public static readonly byte[] kMagicHeader = ReplayWriter.kMagicHeader;
  readonly BinaryReader _reader;

  public ReplayReader(BinaryReader reader) {
    _reader = reader;
  }

  /// <summary>
  /// Reads a 64-bit checksum to the file.
  /// </summary>
  /// <returns>the checksum read from the file.</returns>
  public ulong ReadChecksum() => _reader.ReadULong();

  /// <summary>
  /// Reads a frame of input to the file.
  /// </summary>
  /// <param name="inputs">a frame of input to read.</param>
  public unsafe void ReadInputs(Span<PlayerInput> inputs) {
    Assert.IsTrue(inputs.Length == MatchConfig.kMaxSupportedPlayers);
    fixed (PlayerInput* inputPtr = inputs) {
      var ptr = (byte*)inputPtr;
      int size = MatchConfig.kMaxSupportedPlayers * UnsafeUtility.SizeOf<PlayerInput>();
      ulong checksum = _reader.ReadULong();
      _reader.ReadBytes(ptr, size);
      ulong hash = XXHash.Hash64(ptr, size);
      Assert.AreEqual(checksum, hash);
    }
  }

  /// <summary>
  /// Disposes of the ReplayReader. Also disposes of the underlying
  /// BinaryReader.
  /// </summary>
  public void Dispose() => _reader.Dispose();

}
    
}