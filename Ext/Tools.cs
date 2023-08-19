using PangyaAPI.SuperSocket.Cryptor;
using PangyaAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PangyaAPI.SuperSocket.Ext
{
    public static class Tools
    {
        public static void CleanUp(this byte[] @tmp, Compressor @_compress, Crypt @_crypt)
        {
            if (tmp != null)
            {
                Array.Clear(tmp, 0, tmp.Length);
            }

            _compress = null;
            _crypt?.Dispose();
        }
        public static uint STDA_MAKE_ERROR(STDA_ERROR_TYPE source_error, uint err_code, uint _err_sys)
        {
            return (uint)(STDA_SOURCE_ERROR_ENCODE((uint)source_error) | STDA_ERROR_ENCODE(err_code) | STDA_SYSTEM_ERROR_ENCODE(_err_sys));
        }
        public static uint STDA_MAKE_ERROR(uint source_error, uint err_code, uint _err_sys)
        {
            return (uint)(STDA_SOURCE_ERROR_ENCODE(source_error) | STDA_ERROR_ENCODE(err_code) | STDA_SYSTEM_ERROR_ENCODE(_err_sys));
        }
        static long STDA_SOURCE_ERROR_ENCODE(uint source_error)
        {
            return ((source_error) << 24) & 0xFF000000;
        }

        static long STDA_ERROR_ENCODE(uint err)
        {
            return ((err) << 16) & 0x00FF0000;
        }


        static long STDA_SYSTEM_ERROR_ENCODE(uint _err_sys)
        {
            return (_err_sys) & 0x0000FFFF;
        }

        public static byte[] Clone(this byte[] sourceArray, int startIndex)
        {
            if (sourceArray == null || startIndex < 0 || startIndex >= sourceArray.Length)
                throw new ArgumentException("Invalid input parameters");

            int length = sourceArray.Length - startIndex;
            byte[] newArray = new byte[length];

            Array.Copy(sourceArray, startIndex, newArray, 0, length);

            return newArray;
        }
        public static T ByteArrayToStructure<T>(this byte[] bytes, int index) where T : class
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                IntPtr ptr = IntPtr.Add(handle.AddrOfPinnedObject(), index);
                return (T)Marshal.PtrToStructure(ptr, typeof(T));
            }
            finally
            {
                handle.Free();
            }
        }
        public static byte[] ConvertArray(this object value)

        {
            int size = Marshal.SizeOf(value);
            byte[] arr = new byte[size];

            GCHandle handle = GCHandle.Alloc(arr, GCHandleType.Pinned);
            try
            {
             

                IntPtr ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(value, ptr, false);
                Marshal.Copy(ptr, arr, 0, size);
                Marshal.FreeHGlobal(ptr);
                return arr;
            }
            finally
            {
                handle.Free();
            }
        }

        public static Array Clear(this Array array)
        {
            Array.Clear(array, 0, array.Length);
            return array;
        }

        public static string ExceptionMessage(this Exception ex)
        {
            return "";
        }
        public static byte[] CopyBytes(this byte[] data, int Size)
        {
            var new_data = new byte[Size];

            Buffer.BlockCopy(data, 0, new_data,0, Size);
            return new_data;
        }
        public static void CopyPropertiesTo<T, TU>(this T source, TU dest)
        {
            var sourceProps = typeof(T).GetProperties().Where(x => x.CanRead).ToList();
            var destProps = typeof(TU).GetProperties()
                    .Where(x => x.CanWrite)
                    .ToList();

            foreach (var sourceProp in sourceProps)
            {
                if (destProps.Any(x => x.Name == sourceProp.Name))
                {
                    var p = destProps.First(x => x.Name == sourceProp.Name);
                    if (p.CanWrite)
                    { // check if the property can be set or no.
                        p.SetValue(dest, sourceProp.GetValue(source, null), null);
                    }
                }

            }

        }
    }
}
