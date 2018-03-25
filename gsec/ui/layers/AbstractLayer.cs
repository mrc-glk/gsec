using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.UI;
using gsec;
using gsec.model;

namespace gsec.ui.layers
{
    public abstract class AbstractLayer<T> where T : IDisplayableGeoElement
    {
        protected GraphicsOverlay BaseOverlay;
        public List<T> Elements;

        public AbstractLayer(List<T> elements)
        {
            Elements = elements;
            BaseOverlay = new GraphicsOverlay();
            GenerateGraphics();
        }

        public GraphicsOverlay GetOverlay()
        {
            return BaseOverlay;
        }
        
        public virtual List<Graphic> GetBaseGraphics()
        {
            // linq?
            List<Graphic> graphics = new List<Graphic>();
            foreach (T element in Elements)
            {
                graphics.Add(element.Graphic);
            }

            return graphics;
        }

        public virtual T ByGraphic(Graphic graphic)
        {
            IEnumerable<T> elements = Elements.Where(r => r.Graphic == graphic);
            Console.WriteLine("ByGraphic({0}) found {1} elements", typeof(T).Name, elements.Count());
            return elements.FirstOrDefault();
        }

        public virtual T ByGeometry(Geometry geometry)
        {
            IEnumerable<T> elements = Elements.Where(r => r.Graphic.Geometry == geometry);
            Console.WriteLine("ByGeometry({0}) found {1} elements", typeof(T).Name, elements.Count());
            return elements.FirstOrDefault();
        }

        public virtual T ByPosition(MapPoint point, double toleranceMeters = 0)
        {
            Geometry buf = (toleranceMeters > 0 ? GeometryEngine.BufferGeodetic(point, toleranceMeters, LinearUnits.Meters) : point);

            IEnumerable<T> elements = Elements.Where(r => GeometryEngine.Intersects(r.Graphic.Geometry, buf));
            Console.WriteLine("ByPosition({0}) found {1} elements", typeof(T).Name, elements.Count());
            return elements.FirstOrDefault();
        }

        public virtual T ByID(long id)
        {
            IEnumerable<T> elements = Elements.Where(r => r.ID == id);
            Console.WriteLine("ByID({0}) found {1} elements", typeof(T).Name, elements.Count());
            return elements.FirstOrDefault();
        }

        public virtual T GetNearestTo(MapPoint point)
        {
            MapPoint nearestCoord = GeoUtil.GetNearestCoordinateInGraphicsCollection(point, GetBaseGraphics());
            return ByPosition(nearestCoord);
        }

        public virtual void Select(T element)
        {
            // do nothing by default
        }

        public virtual void Unselect(T element)
        {
            // do nothing by default
        }

        public virtual void GenerateGraphics()
        {
            // in case of regeneration
            BaseOverlay.Graphics.Clear();
        }
        
        public virtual void RemoveAll()
        {
            foreach (T element in Elements)
            {
                RemoveElement(element);
            }
        }        

        public virtual void AddElement(MapPoint position)
        {
        }

        public virtual void SetVisible(bool visible)
        {
            BaseOverlay.IsVisible = visible;
        }

        protected abstract void GenerateGraphicFor(T element);
        public abstract void RemoveElement(T element);
    }
}
