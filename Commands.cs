using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
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
    public static class SectionExtents
    {
        public static int AppendSectionGeometry(this Section section, ObjectId sourceId, ObjectId btrId)
        {
            Database db = section.Database;
            db.Ltscale = 0.1;
            Matrix3d transform = Matrix3d.AlignCoordinateSystem(
                    section.Boundary[0],
                    section.Boundary[0].GetVectorTo(section.Boundary[1]).GetNormal(),
                    section.VerticalDirection,
                    section.ViewingDirection,
                    Point3d.Origin,
                    Vector3d.XAxis,
                    Vector3d.YAxis,
                    Vector3d.ZAxis
                );

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                SectionSettings settings = (SectionSettings)tr.GetObject(section.Settings, OpenMode.ForWrite);
                var btr = (BlockTableRecord)tr.GetObject(btrId, OpenMode.ForWrite);
                var solid = (Solid3d)tr.GetObject(sourceId, OpenMode.ForRead);
                bool isForeGroundVisible = true;
                bool isHiddenLineVisible = true;
                settings.SetVisibility(SectionType.Section2d, SectionGeometry.ForegroundGeometry, isForeGroundVisible);
                settings.SetHiddenLine(SectionType.Section2d, SectionGeometry.ForegroundGeometry, isHiddenLineVisible);
                bool isDivisionLines = true;
                settings.SetDivisionLines(SectionType.Section2d, SectionGeometry.IntersectionBoundary, isDivisionLines);
                settings.SetColor(SectionType.Section2d, SectionGeometry.IntersectionBoundary, Color.FromColorIndex(ColorMethod.ByColor, 1));
                settings.SetLinetype(SectionType.Section2d, SectionGeometry.IntersectionBoundary, "DASHED");

                //customize the section using the settings object
                settings.CurrentSectionType = SectionType.Section2d;
                settings.SetSourceObjects(SectionType.Section2d, new ObjectIdCollection() { sourceId });
                settings.SetGenerationOptions(SectionType.Section2d, SectionGeneration.SourceSelectedObjects | SectionGeneration.DestinationNewBlock);

                List<Entity> entities = new List<Entity>();


                section.GenerateSectionGeometry(solid,
                                                out Array intersectionBoundary,
                                                out Array intersectionFillAnnotation,
                                                out Array background,
                                                out Array foreground,
                                                out Array curveTangency);

                foreach (Entity e in intersectionBoundary)
                    entities.Add(e);
                foreach (Entity e in intersectionFillAnnotation)
                    entities.Add(e);
                foreach (Entity e in background)
                    entities.Add(e);
                foreach (Entity e in foreground)
                    entities.Add(e);                                  
                foreach (Entity e in curveTangency)
                    entities.Add(e);


                foreach (Entity ent in entities)
                {
                    ent.TransformBy(transform);
                    btr.AppendEntity(ent);
                    tr.AddNewlyCreatedDBObject(ent, true);
                }

                tr.Commit();
            }

            return 0;
        }
    }
    public class Commands
    {
        [CommandMethod("SECTION2D")]
        public static void Section2d()
        {
            var doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            var db = HostApplicationServices.WorkingDatabase;
            var opt = new PromptEntityOptions("Select section object");
            opt.SetRejectMessage("Please select a section.");
            opt.AddAllowedClass(typeof(Section), false);
            var res = doc.Editor.GetEntity(opt);
            if (res.Status != Autodesk.AutoCAD.EditorInput.PromptStatus.OK)
                return;
            var sectionId = res.ObjectId;
            opt = new PromptEntityOptions("Select solid object");
            opt.SetRejectMessage("Please select a solid.");
            opt.AddAllowedClass(typeof(Solid3d), false);
            res = doc.Editor.GetEntity(opt);
            if (res.Status != Autodesk.AutoCAD.EditorInput.PromptStatus.OK)
                return;
            var solidId = res.ObjectId;
            using (var t = doc.TransactionManager.StartOpenCloseTransaction())
            {
                var section = (Section)t.GetObject(sectionId, OpenMode.ForRead);
                var solid = (Solid3d)t.GetObject(solidId, OpenMode.ForRead);
                section.AppendSectionGeometry(solidId, SymbolUtilityServices.GetBlockModelSpaceId(db));
                t.Commit();
            }
        }

        [CommandMethod("-SP2B")]
        public static void Sp2B()
        {
            var doc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            var db = HostApplicationServices.WorkingDatabase;            
            var opt = new PromptEntityOptions("Select section object");
            opt.SetRejectMessage("Please select a section.");
            opt.AddAllowedClass(typeof(Section), false);
            var res = doc.Editor.GetEntity(opt);
            if (res.Status != Autodesk.AutoCAD.EditorInput.PromptStatus.OK)
                return;
            var sectionId = res.ObjectId;
            opt = new PromptEntityOptions("Select solid object");
            opt.SetRejectMessage("Please select a solid.");
            opt.AddAllowedClass(typeof(Solid3d), false);
            res = doc.Editor.GetEntity(opt);
            if (res.Status != Autodesk.AutoCAD.EditorInput.PromptStatus.OK)
                return;
            var solidId = res.ObjectId;
            using (var t = doc.TransactionManager.StartOpenCloseTransaction())
            {
                var section = (Section)t.GetObject(sectionId, OpenMode.ForRead);
                var settings = (SectionSettings)t.GetObject(section.Settings, OpenMode.ForWrite);
                var solid = (Solid3d)t.GetObject(solidId, OpenMode.ForRead);
                //customize the section using the settings object
                bool isForeGroundVisible = true;
                bool isHiddenLineVisible = true;
                settings.SetVisibility(SectionType.Section2d, SectionGeometry.ForegroundGeometry, isForeGroundVisible);
                settings.SetHiddenLine(SectionType.Section2d, SectionGeometry.ForegroundGeometry, isHiddenLineVisible);
                bool isDivisionLines = true;
                settings.SetDivisionLines(SectionType.Section2d, SectionGeometry.IntersectionBoundary, isDivisionLines);
                settings.SetColor(SectionType.Section2d, SectionGeometry.IntersectionBoundary, Color.FromColorIndex(ColorMethod.ByColor, 1));
                settings.SetLinetype(SectionType.Section2d, SectionGeometry.IntersectionBoundary, "DASHED");
                //customize the section using the settings object
                settings.CurrentSectionType = SectionType.Section2d;
                settings.SetSourceObjects(SectionType.Section2d, new ObjectIdCollection() { solidId });
                settings.SetGenerationOptions(SectionType.Section2d, SectionGeneration.SourceSelectedObjects | SectionGeneration.DestinationNewBlock);                
                //etc.
                t.Commit();
            }
            if (!SystemObjects.DynamicLinker.IsModuleLoaded("acsection.crx"))
                SystemObjects.DynamicLinker.LoadModule("acsection.crx", false, true);
            CliSectionPlaneToBlock.Run(res.ObjectId);
        }
    }
}

