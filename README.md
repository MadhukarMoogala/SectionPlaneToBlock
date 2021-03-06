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

 We also created `SECTION2D` command to show how to use Section .NET API to create sectioning geomerties.

### DEMO

![SECTIONIMAGE](https://github.com/MadhukarMoogala/SectionPlaneToBlock/blob/master/SectionDemo.gif)

 ### Nota Bene
 After running SP2B, if you SECTIONPLANETOBLOCK command, application crashes.
 The intent of SP2B is to use in AcCore environment or automation projects.


# Simulating a C++ interface in C#

The code also demonstrates how to define C++ interface in C#. i.e. manually laying out a vtable. This could be generalized, interestingly I couldn't find any examples like this on the interweb.

### License
This sample is licensed under the terms of the [MIT License](http://opensource.org/licenses/MIT).

### Written by
Albert Szilvasy, Madhukar Moogala

