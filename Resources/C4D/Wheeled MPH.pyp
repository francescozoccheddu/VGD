from __future__ import division
import c4d
from c4d import gui
import c4d
from c4d import gui
from c4d import utils
from c4d import plugins
from c4d import documents

PLUGIN_ID=1029596

def ensureFits(value, bits):
    if not isinstance(value, int):
        raise ValueError("Non int value")
    if value < 0:
        raise ValueError("Negative value")
    if value.bit_length() > bits:
        raise ValueError("Value overflow")

def encodeBin(material, emission, tint):
    ensureFits(material, 3)
    ensureFits(emission, 2)
    ensureFits(tint, 1)
    return (material << 0) | (emission << 3) | (tint << 5)

def encode(material, emission, tint):
    bin = (encodeBin(material, emission, tint) << 2) + (1 << 1)
    return bin / (1 << 8)

def decode(alpha):
    if not 0 <= alpha <= 1:
        raise ValueError("Value does not fall in range [0,1]")
    bin8 = min(alpha * (1 << 8), (1 << 8) - 1)
    bin = int(bin8 / (1 << 2))
    material = (bin >> 0) & ((1 << 3) - 1)
    emission = (bin >> 3) & ((1 << 2) - 1)
    tint = (bin >> 5) & ((1 << 1) - 1)
    return material, emission, tint

class Dialog(c4d.gui.GeDialog):

    GROUP_MATERIAL = 100011
    GROUP_EMISSION = 100012
    GROUP_TINT = 100013
    GROUP_RESULT = 100014
    CONTROL_MATERIAL = 100015
    CONTROL_EMISSION = 100016
    CONTROL_TINT = 100017
    CONTROL_RESULT = 100018
    BUTTON_EXPORT = 100019
    BUTTON_IMPORT = 100020

    def CreateLayout(self):
        self.SetTitle ("Wheeled MPH")

        spacing = 4

        self.GroupBegin(id=Dialog.GROUP_MATERIAL, cols=1, rows=2, title="Material", flags=c4d.BFH_SCALEFIT)
        self.GroupBorder(c4d.BORDER_WITH_TITLE)
        self.GroupBorderSpace(spacing, spacing, spacing, 0)
        self.AddEditSlider(id=Dialog.CONTROL_MATERIAL, flags=c4d.BFH_SCALEFIT)
        self.SetInt32(id=Dialog.CONTROL_MATERIAL, value=1, min=1, max=8)
        self.GroupEnd()

        self.GroupBegin(id=Dialog.GROUP_EMISSION, cols=1, rows=2, title="Emission", flags=c4d.BFH_SCALEFIT)
        self.GroupBorder(c4d.BORDER_WITH_TITLE)
        self.GroupBorderSpace(spacing, spacing, spacing, 0)
        self.AddComboBox(id=Dialog.CONTROL_EMISSION, flags=c4d.BFH_SCALEFIT)
        self.AddChild(id=Dialog.CONTROL_EMISSION, subid=0, child="None")
        self.AddChild(id=Dialog.CONTROL_EMISSION, subid=1, child="Half")
        self.AddChild(id=Dialog.CONTROL_EMISSION, subid=2, child="Full")
        self.AddChild(id=Dialog.CONTROL_EMISSION, subid=3, child="Overshoot")
        self.GroupEnd()

        self.GroupBegin(id=Dialog.GROUP_TINT, cols=1, rows=1, flags=c4d.BFH_SCALEFIT)
        self.GroupBorderNoTitle(c4d.BORDER_NONE)
        self.GroupBorderSpace(spacing, spacing, spacing, 0)
        self.AddCheckbox(id=Dialog.CONTROL_TINT, name="Tint", flags=c4d.BFH_LEFT, initw=0, inith=0)
        self.GroupEnd()

        self.AddSeparatorH(inith=0)

        self.GroupBegin(id=Dialog.GROUP_RESULT, cols=3, rows=1, title="Result", flags=c4d.BFH_SCALEFIT)
        self.GroupBorder(c4d.BORDER_WITH_TITLE_BOLD)
        self.GroupBorderSpace(spacing, spacing, spacing, spacing)
        self.AddEditNumberArrows(id=Dialog.CONTROL_RESULT, flags=c4d.BFH_SCALEFIT)
        self.SetPercent(id=Dialog.CONTROL_RESULT, value=0, min=0, max=100, step=1)
        self.AddButton(id=Dialog.BUTTON_EXPORT, name="Export", flags=c4d.BFH_LEFT)
        self.AddButton(id=Dialog.BUTTON_IMPORT, name="Import", flags=c4d.BFH_LEFT)
        self.GroupEnd()

        return True

    def doUpdate(self):
        material = self.GetInt32(id=Dialog.CONTROL_MATERIAL) - 1
        emission = self.GetInt32(id=Dialog.CONTROL_EMISSION)
        tint = self.GetBool(id=Dialog.CONTROL_TINT)
        value = encode(material, emission, tint)
        self.SetPercent(id=Dialog.CONTROL_RESULT, value=value, min=0, max=100, step=1)

    def doUpdateBack(self):
        value = self.GetFloat(id=Dialog.CONTROL_RESULT)
        material, emission, tint = decode(value)
        self.SetInt32(id=Dialog.CONTROL_MATERIAL, value=material + 1, min=1, max=8, step=1)
        self.SetInt32(id=Dialog.CONTROL_EMISSION, value=emission, min=0, max=3, step=1)
        self.SetBool(id=Dialog.CONTROL_TINT, value=tint)

    def doExport(self):
        value = self.GetFloat(id=Dialog.CONTROL_RESULT)
        doc = documents.GetActiveDocument()
        data = plugins.GetToolData(doc, 1021286)
        data[c4d.ID_CA_PAINT_TOOL_VERTEXCOLOR_APLHAVALUE] = value
        mode = data[c4d.ID_CA_PAINT_TOOL_MAINMODE]
        if mode == 0:
            data[c4d.ID_CA_PAINT_TOOL_MAINMODE] = 3
        elif mode == 1:
            data[c4d.ID_CA_PAINT_TOOL_MAINMODE] = 2
        data[c4d.ID_CA_PAINT_TOOL_COLORMODE] = 0
        doc.SetAction(1021286)

    def doImport(self):
        doc = documents.GetActiveDocument()
        value = plugins.GetToolData(doc, 1021286)[c4d.ID_CA_PAINT_TOOL_VERTEXCOLOR_APLHAVALUE]
        self.SetPercent(id=Dialog.CONTROL_RESULT, value=value, min=0, max=100, step=1)

    def Command(self, id, msg):
        if id == Dialog.BUTTON_EXPORT:
            self.doExport()
        elif id == Dialog.BUTTON_IMPORT:
            self.doImport()
            self.doUpdateBack()
        elif id == Dialog.CONTROL_RESULT:
            self.doUpdateBack()
            self.doExport()
        elif id in [Dialog.CONTROL_MATERIAL, Dialog.CONTROL_EMISSION, Dialog.CONTROL_TINT]:
            self.doUpdate()
            self.doExport()
        return True


class Menu(c4d.plugins.CommandData):

    dialog = None
    
    def Execute(self, doc):
       if self.dialog is None:
          self.dialog = Dialog ()
       return self.dialog.Open (dlgtype=c4d.DLG_TYPE_ASYNC, pluginid=PLUGIN_ID)

    def RestoreLayout(self, sec_ref):
        if self.dialog is None:
            self.dialog = Dialog()
        return self.dialog.Restore (pluginid=PLUGIN_ID, secret=sec_ref)

def main():
    ok = plugins.RegisterCommandPlugin (PLUGIN_ID, "Wheeled MPH", 0, None, "Wheeled", Menu())
    if not ok:
        print ("[Wheeled MPH] Failed to initialize")

if __name__=='__main__':
    main()