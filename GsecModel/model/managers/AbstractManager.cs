using NHibernate;
using NHibernate.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gsec.model.managers
{
    public interface IHManager
    {
        string GetTable();
        string GetIdColumn();
        string GetGeometryColumn();
    }

    public abstract class AbstractManager<T> : IHManager where T : IDisplayableGeoElement
    {
        protected static PersistentClass PersistentClass { get; set; }

        static AbstractManager()
        {
            PersistentClass = HSession.ClassMappings.Where(x => x.EntityName == typeof(T).FullName).Single();
        }

        public string GetTable()
        {
            return PersistentClass.Table.Name;
        }

        public string GetIdColumn()
        {
            return PersistentClass.Identifier.ColumnIterator.Single().Text;
        }

        public string GetGeometryColumn()
        {
            return "geom";
        }

        public IList<T> List()
        {
            ISession session = HSession.Factory.OpenSession();
            IList<T> list = null;

            try
            {
                session.BeginTransaction();
                list = session.CreateQuery("from " + typeof(T).Name).List<T>();
                session.Transaction.Commit();
            }
            catch (HibernateException e)
            {
                Console.WriteLine(e.Message);
                session.Transaction.Rollback();
                throw new GsecException("failed to hibernate");
            }
            finally
            {
                session.Close();
            }

            return list;
        }

        public long Create(T obj)
        {
            ISession session = HSession.Factory.OpenSession();
            long id = -1;

            try
            {
                session.BeginTransaction();
                id = (long)session.Save(obj);
                session.Transaction.Commit();
                obj.ID = id;
            }
            catch (HibernateException e)
            {
                Console.WriteLine(e.Message);
                session.Transaction.Rollback();
                throw new GsecException("failed to hibernate");
            }
            finally
            {
                session.Close();
            }

            return id;
        }

        public T Read(long id)
        {
            ISession session = HSession.Factory.OpenSession();
            T obj = default(T);

            try
            {
                session.BeginTransaction();
                obj = (T) session.Get(typeof(T), id);
                session.Transaction.Commit();
            }
            catch (HibernateException e)
            {
                Console.WriteLine(e.Message);
                session.Transaction.Rollback();
                throw new GsecException("failed to hibernate");
            }
            finally
            {
                session.Close();
            }

            return obj;
        }

        public void Update(T obj)
        {
            ISession session = HSession.Factory.OpenSession();

            try
            {
                session.BeginTransaction();
                session.Update(obj);
                session.Transaction.Commit();
            }
            catch (HibernateException e)
            {
                Console.WriteLine(e.Message);
                session.Transaction.Rollback();
                throw new GsecException("failed to hibernate");
            }
            finally
            {
                session.Close();
            }
        }

        public void Delete(T obj)
        {
            ISession session = HSession.Factory.OpenSession();

            try
            {
                session.BeginTransaction();
                //session.Delete(obj);
                var queryString = string.Format("DELETE {0} WHERE id = :id", typeof(T));
                session.CreateQuery(queryString)
                       .SetParameter("id", obj.ID)
                       .ExecuteUpdate();

                session.Transaction.Commit();
            }
            catch (HibernateException e)
            {
                Console.WriteLine(e.Message);
                session.Transaction.Rollback();
                throw new GsecException("failed to hibernate");
                //Console.WriteLine("!! Failed to hibernate");
            }
            finally
            {
                session.Close();
            }
        }
    }
}
