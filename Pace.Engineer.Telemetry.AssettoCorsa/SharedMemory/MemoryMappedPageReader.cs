using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace Pace.Engineer.Telemetry.AssettoCorsa.SharedMemory;

internal sealed class MemoryMappedPageReader<T> : IDisposable where T : struct
{
    private readonly string _mapName;
    private MemoryMappedFile? _mmf;
    private MemoryMappedViewAccessor? _accessor;

    private readonly int _size = Marshal.SizeOf<T>();

    public MemoryMappedPageReader(string mapName)
    {
        _mapName = mapName;
    }

    public bool TryConnect()
    {
        if (_accessor != null)
        {
            return true;
        }

        try
        {
            _mmf = MemoryMappedFile.OpenExisting(_mapName, MemoryMappedFileRights.Read);
            _accessor = _mmf.CreateViewAccessor(0, _size, MemoryMappedFileAccess.Read);
            return true;
        }
        catch
        {
            DisposeHandles();
            return false;
        }
    }

    public bool TryRead(out T value)
    {
        value = default;

        if (!TryConnect())
        {
            return false;
        }

        try
        {
            var buffer = new byte[_size];
            _accessor!.ReadArray(0, buffer, 0, buffer.Length);

            var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
            try
            {
                value = Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
                return true;
            }
            finally
            {
                handle.Free();
            }
        }
        catch
        {
            DisposeHandles();
            return false;
        }
    }

    public void Dispose()
    {
        DisposeHandles();
    }

    private void DisposeHandles()
    {
        _accessor?.Dispose();
        _mmf?.Dispose();
        _accessor = null;
        _mmf = null;
    }
}