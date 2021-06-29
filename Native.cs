using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SectionPlaneToBlock
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    struct IAcApHostDialog
    {
        IntPtr pvtable; //vtable pointer
        IntPtr vtable; //vtable (1 entry)
        public IntPtr objectId;
        public static unsafe void Init(IntPtr* this_, ShowDialog showDialog, ObjectId id)
        {
            //initialize vtable
            this_[1] = Marshal.GetFunctionPointerForDelegate(showDialog);
            //initialize vtable pointer to point to vtable
            this_[0] = new IntPtr(&this_[1]);
            this_[2] = id.OldIdPtr;
        }
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        public delegate int ShowDialog(ref IAcApHostDialog this_, IntPtr parameters);
    }
    class NativeMethods
    {
        [DllImport(
            dllName: "accore.dll",
            CallingConvention = CallingConvention.ThisCall,
            ExactSpelling = true,
            CharSet = CharSet.Unicode,
            EntryPoint = "?setHost@AcApHostDialog@@QEAAXPEAVIAcApHostDialog@@@Z"),
        ]
        public extern static void AcApHostDialog_setHost(IntPtr this_, IntPtr host);

        [DllImport(
            dllName: "acsection.crx",
            CallingConvention = CallingConvention.Cdecl,
            ExactSpelling = true,
            CharSet = CharSet.Unicode,
            EntryPoint = "?instance@AcApHostDialogSectionGenerateSettings@@SAPEAV1@XZ"),
        ]
        public extern static IntPtr AcApHostDialogSectionGenerateSettings_instance();

        [DllImport(
            dllName: "acsection.crx",
            CallingConvention = CallingConvention.ThisCall,
            ExactSpelling = true,
            CharSet = CharSet.Unicode,
            EntryPoint = "?setSectionId@AcApHostSectionGenerateSettingsDialogServices@@QEAAXAEBVAcDbObjectId@@@Z"),
        ]
        public extern static IntPtr AcApHostSectionGenerateSettingsDialogServices_setSectionId(IntPtr this_, ref IntPtr objectId);

        [DllImport(
            dllName: "accore.dll",
            CallingConvention = CallingConvention.ThisCall,
            ExactSpelling = true,
            CharSet = CharSet.Unicode,
            EntryPoint = "?setReturnCode@AcApHostDialogParams@@QEAAX_J@Z"),
        ]
        public extern static void AcApHostDialogParams_setReturnCode(IntPtr this_, IntPtr retCode);
        [DllImport(
            dllName: "acsection.crx",
            CallingConvention = CallingConvention.ThisCall,
            ExactSpelling = true,
            CharSet = CharSet.Unicode,
            EntryPoint = "?getDialogServices@AcApHostSectionGenerateSettingsDialogParams@@QEBAPEAVAcApHostSectionGenerateSettingsDialogServices@@XZ"),
        ]
        public extern static IntPtr AcApHostSectionGenerateSettingsDialogParams_getDialogServices(IntPtr this_);
    }

    class CliSectionPlaneToBlock : IDisposable
    {
        IAcApHostDialog m_native = new IAcApHostDialog();
        IntPtr m_ptr;
        GCHandle m_pin;
        IAcApHostDialog.ShowDialog m_delegate;
        static int ShowDialog(ref IAcApHostDialog this_, IntPtr parameters)
        {
            NativeMethods.AcApHostDialogParams_setReturnCode(parameters, new IntPtr(1));
            var svc = NativeMethods.AcApHostSectionGenerateSettingsDialogParams_getDialogServices(parameters);
            NativeMethods.AcApHostSectionGenerateSettingsDialogServices_setSectionId(svc, ref this_.objectId);
            return 0;
        }
        unsafe CliSectionPlaneToBlock(ObjectId section)
        {
            m_pin = GCHandle.Alloc(m_native, GCHandleType.Pinned);
            m_ptr = m_pin.AddrOfPinnedObject();
            m_delegate = new IAcApHostDialog.ShowDialog(ShowDialog);
            IAcApHostDialog.Init((IntPtr*)m_ptr.ToPointer(), m_delegate, section);
        }
        IntPtr NativePtr { get { return m_ptr; } }
        public static void Run(ObjectId section)
        {
            using (var fake = new CliSectionPlaneToBlock(section))
            {
                var point3d = Point3d.Origin;
                using (var oct = new OpenCloseTransaction())
                {
                    var sec = (Section)oct.GetObject(section, OpenMode.ForRead);
                    Matrix3d transform = Matrix3d.AlignCoordinateSystem(
                                         sec.Boundary[0],
                                         sec.Boundary[0].GetVectorTo(sec.Boundary[1]).GetNormal(),
                                         sec.VerticalDirection,
                                         sec.ViewingDirection,
                                         Point3d.Origin,
                                         Vector3d.XAxis,
                                         Vector3d.YAxis,
                                         Vector3d.ZAxis
                                         );
                    point3d = point3d.TransformBy(transform);
                    oct.Commit();
                }
                //provide AcSection.crx with a fake dialog implementation. This allows us to reuse the 
                //block generation code.
                var dlgProxy = NativeMethods.AcApHostDialogSectionGenerateSettings_instance();
                NativeMethods.AcApHostDialog_setHost(dlgProxy, fake.NativePtr);
                var ed = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument.Editor;
                var xScale = 1;
                var yScale = xScale;
                var rot = 0;
                ed.Command("_SECTIONPLANETOBLOCK", point3d, xScale, yScale, rot);
            


            }
        }
        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                m_pin.Free();
                disposedValue = true;
            }
        }
        ~CliSectionPlaneToBlock()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
