using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

[assembly: ExtensionApplication(null)]
[assembly: CommandClass(typeof(SectionPlaneToBlock.Commands))]

namespace SectionPlaneToBlock
{
    public class Commands
    {
        [CommandMethod("-SP2B")]
        public static void Sp2B()
        {
            var doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            var opt = new PromptEntityOptions("Select section object");
            opt.SetRejectMessage("Please select a section.");
            opt.AddAllowedClass(typeof(Section), false);
            var res = doc.Editor.GetEntity(opt);
            if (res.Status != Autodesk.AutoCAD.EditorInput.PromptStatus.OK)
                return;
            using (var t = doc.TransactionManager.StartOpenCloseTransaction())
            {
                var section = (Section)t.GetObject(res.ObjectId, OpenMode.ForRead);
                var settings = (SectionSettings)t.GetObject(section.Settings, OpenMode.ForWrite);
                //customize the section using the settings object
                settings.CurrentSectionType = SectionType.Section2d;
                //etc.
                t.Commit();
            }
            if (!SystemObjects.DynamicLinker.IsModuleLoaded("acsection.crx"))
                SystemObjects.DynamicLinker.LoadModule("acsection.crx", false, true);
            CliSectionPlaneToBlock.Run(res.ObjectId);
        }
    }
}
