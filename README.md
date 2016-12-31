# SectionPlaneToBlock
Provides a workaround for the lack of CLI version for the SECTIONPLANETOBLOCK command

Here's how it works:

1. We define a `-SP2B` command.
2. The command registers a fake dialog with acsection.crx. This fake dialog simply does nothing when `showDialog` is called.
3. The command then calls the real `SECTIONPLANETOBLOCK` command which is implemented roughly like this in acsection.crx:
```
void SectionPlaneToBlock()
{
    AcDbObjectId objId;
    AcApHostSectionGenerateSettingsDialogServices services(objId);
    AcApHostSectionGenerateSettingsDialogParams params(&services);
    if (eOkTest(AcApHostDialogSectionGenerateSettings::instance()->showDialog(&params)) && params.returnCode() == IDOK)
        AcSectionUtil::GenerateSection(services.getSectionId());
}
```

The point of the fake dialog is to ensure that we end up calling `AcSectionUtil::GenerateSection(...)`. This is the key function that we want reuse.

# Simulating a C++ interface in C#

The code also demonstrates how to define C++ interface in C#. i.e. manually laying out a vtable. This could be generalized, interestingly I couldn't find any examples like this on the interweb.
