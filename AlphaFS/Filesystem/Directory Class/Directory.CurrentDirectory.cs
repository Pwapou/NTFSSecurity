/*  Copyright (C) 2008-2016 Peter Palotas, Jeffrey Jangli, Alexandr Normuradov
 *  
 *  Permission is hereby granted, free of charge, to any person obtaining a copy 
 *  of this software and associated documentation files (the "Software"), to deal 
 *  in the Software without restriction, including without limitation the rights 
 *  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
 *  copies of the Software, and to permit persons to whom the Software is 
 *  furnished to do so, subject to the following conditions:
 *  
 *  The above copyright notice and this permission notice shall be included in 
 *  all copies or substantial portions of the Software.
 *  
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 *  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN 
 *  THE SOFTWARE. 
 */

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace Alphaleonis.Win32.Filesystem
{
   partial class Directory
   {
      /// <summary>
      /// Gets the current working directory of the application.
      /// <para>
      ///   MSDN: Multithreaded applications and shared library code should not use the GetCurrentDirectory function and should avoid using relative path names.
      ///   The current directory state written by the SetCurrentDirectory function is stored as a global variable in each process,
      ///   therefore multithreaded applications cannot reliably use this value without possible data corruption from other threads that may also be reading or setting this value.
      ///   <para>This limitation also applies to the SetCurrentDirectory and GetFullPathName functions. The exception being when the application is guaranteed to be running in a single thread,
      ///   for example parsing file names from the command line argument string in the main thread prior to creating any additional threads.</para>
      ///   <para>Using relative path names in multithreaded applications or shared library code can yield unpredictable results and is not supported.</para>
      /// </para>
      /// </summary>
      /// <returns>The path of the current working directory without a trailing directory separator.</returns>
      [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate"), SecurityCritical]
      public static string GetCurrentDirectory()
      {
         var nameBuffer = new StringBuilder(NativeMethods.MaxPathUnicode);

         // SetCurrentDirectory()
         // In the ANSI version of this function, the name is limited to 248 characters.
         // To extend this limit to 32,767 wide characters, call the Unicode version of the function and prepend "\\?\" to the path.
         // 2016-09-29: MSDN does not confirm LongPath usage but a Unicode version of this function exists.

         var folderNameLength = NativeMethods.GetCurrentDirectory((uint) nameBuffer.Capacity, nameBuffer);
         var lastError = Marshal.GetLastWin32Error();

         if (folderNameLength == 0)
            NativeError.ThrowException(lastError);

         if (folderNameLength > NativeMethods.MaxPathUnicode)
            throw new PathTooLongException(string.Format(CultureInfo.InvariantCulture, "Path is greater than {0} characters: {1}", NativeMethods.MaxPathUnicode, folderNameLength));

         return nameBuffer.ToString();
      }


      /// <summary>
      /// Sets the application's current working directory to the specified directory.
      /// <para>
      ///   MSDN: Multithreaded applications and shared library code should not use the GetCurrentDirectory function and should avoid using relative path names.
      ///   The current directory state written by the SetCurrentDirectory function is stored as a global variable in each process,
      ///   therefore multithreaded applications cannot reliably use this value without possible data corruption from other threads that may also be reading or setting this value.
      ///   <para>This limitation also applies to the SetCurrentDirectory and GetFullPathName functions. The exception being when the application is guaranteed to be running in a single thread,
      ///   for example parsing file names from the command line argument string in the main thread prior to creating any additional threads.</para>
      ///   <para>Using relative path names in multithreaded applications or shared library code can yield unpredictable results and is not supported.</para>
      /// </para>
      /// </summary>
      /// <param name="path">The path to which the current working directory is set.</param>
      [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Utils.IsNullOrWhiteSpace validates arguments.")]
      [SecurityCritical]
      public static void SetCurrentDirectory(string path)
      {
         SetCurrentDirectory(path, PathFormat.RelativePath);
      }


      /// <summary>
      /// Sets the application's current working directory to the specified directory.
      /// <para>
      ///   MSDN: Multithreaded applications and shared library code should not use the GetCurrentDirectory function and should avoid using relative path names.
      ///   The current directory state written by the SetCurrentDirectory function is stored as a global variable in each process,
      ///   therefore multithreaded applications cannot reliably use this value without possible data corruption from other threads that may also be reading or setting this value.
      ///   <para>This limitation also applies to the SetCurrentDirectory and GetFullPathName functions. The exception being when the application is guaranteed to be running in a single thread,
      ///   for example parsing file names from the command line argument string in the main thread prior to creating any additional threads.</para>
      ///   <para>Using relative path names in multithreaded applications or shared library code can yield unpredictable results and is not supported.</para>
      /// </para>
      /// </summary>
      /// <param name="path">The path to which the current working directory is set.</param>
      /// <param name="pathFormat">Indicates the format of the path parameter.</param>
      [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Utils.IsNullOrWhiteSpace validates arguments.")]
      [SecurityCritical]
      public static void SetCurrentDirectory(string path, PathFormat pathFormat)
      {
         if (Utils.IsNullOrWhiteSpace(path))
            throw new ArgumentNullException("path");

         var fullCheck = pathFormat == PathFormat.RelativePath;
         Path.CheckSupportedPathFormat(path, fullCheck, fullCheck);
         var pathLp = Path.GetExtendedLengthPathCore(null, path, pathFormat, GetFullPathOptions.AddTrailingDirectorySeparator);

         if (pathFormat == PathFormat.FullPath)
            pathLp = Path.GetRegularPathCore(pathLp, GetFullPathOptions.None, false);


         // SetCurrentDirectory()
         // In the ANSI version of this function, the name is limited to 248 characters.
         // To extend this limit to 32,767 wide characters, call the Unicode version of the function and prepend "\\?\" to the path.
         // 2016-09-29: MSDN confirms LongPath usage starting with Windows 10, version 1607.

         if (!NativeMethods.SetCurrentDirectory(pathLp))
            NativeError.ThrowException(Marshal.GetLastWin32Error(), pathLp);
      }
   }
}
