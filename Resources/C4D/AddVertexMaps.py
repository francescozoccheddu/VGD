import c4d
from c4d import documents

tagnames = ["Wheeled emission map", "Wheeled tint map"]

def main():
    doc = documents.GetActiveDocument()
    objs = doc.GetActiveObjects(c4d.GETACTIVEOBJECTFLAGS_CHILDREN)
    doc.StartUndo()
    for obj in objs:
        if obj.GetType() == c4d.Opolygon:
            reqtagnames = list(tagnames)
            for tag in obj.GetTags():
                if tag.GetType() == c4d.Tvertexmap:
                    tagname = tag.GetName().strip()
                    try:
                        reqtagnames.remove(tagname)
                    except:
                        pass
            for tagname in reqtagnames:
                tag = obj.MakeVariableTag(c4d.Tvertexmap, obj.GetPointCount())
                tag.SetName(tagname)
                doc.AddUndo(c4d.UNDOTYPE_NEW, tag)
    c4d.EventAdd()
    doc.EndUndo()

if __name__=='__main__':
    main()
