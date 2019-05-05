import c4d
from c4d import documents

tagnames = ["Wheeled emission map", "Wheeled tint map"]
defaults = [0.0, 0.0]

def calcCoord(data, default, index):
    return default if data is None else data[index]

def calcUV(data, index):
    x = calcCoord(data[0], defaults[0], index)
    y = calcCoord(data[1], defaults[1], index)
    return c4d.Vector(x, y, 0.0)

def main():
    doc = documents.GetActiveDocument()
    objs = doc.GetActiveObjects(c4d.GETACTIVEOBJECTFLAGS_CHILDREN)
    doc.StartUndo()
    for obj in objs:
        if obj.GetType() == c4d.Opolygon:
            tags = [None, None]
            for tag in obj.GetTags():
                if tag.GetType() == c4d.Tvertexmap:
                    tagname = tag.GetName().strip()
                    try:
                        index = tagnames.index(tagname)
                        tags[index] = tag
                    except:
                        pass
                elif tag.GetType() == c4d.Tuvw:
                    doc.AddUndo(c4d.UNDOTYPE_DELETE, tag)
                    tag.Remove()
            if any(tags):
                tag = obj.MakeVariableTag(c4d.Tuvw, obj.GetPolygonCount())
                doc.AddUndo(c4d.UNDOTYPE_NEW, tag)
                tag[c4d.UVWTAG_LOCK] = 1
                tag.SetName("Wheeled tint/emission map")
                data = list(map(lambda tag: None if tag is None else tag.GetAllHighlevelData(), tags))
                for index, poly in enumerate(obj.GetAllPolygons()):
                    a = calcUV(data, poly.a)
                    b = calcUV(data, poly.b)
                    c = calcUV(data, poly.c)
                    d = c4d.Vector() if poly.IsTriangle() else calcUV(data, poly.d)
                    tag.SetSlow(index, a, b, c, d)
    c4d.EventAdd()
    doc.EndUndo()

if __name__=='__main__':
    main()
