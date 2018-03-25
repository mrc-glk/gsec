using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gsec.model.managers
{
    public abstract class AbstractManager<T> where T : class
    {
        public abstract string GetTable();

        public IList<T> List()
        {
            ISession session = HSession.Factory.OpenSession();
            IList<T> list = null;

            try
            {
                session.BeginTransaction();
                list = session.CreateQuery("from " + GetTable()).List<T>();
                session.Transaction.Commit();
            }
            catch (HibernateException e)
            {
                Console.WriteLine(e.Message);
                session.Transaction.Rollback();
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
            }
            catch (HibernateException e)
            {
                Console.WriteLine(e.Message);
                session.Transaction.Rollback();
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
            T obj = null;

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
                session.Delete(obj);
                session.Transaction.Commit();
            }
            catch (HibernateException e)
            {
                Console.WriteLine(e.Message);
                session.Transaction.Rollback();
            }
            finally
            {
                session.Close();
            }
        }
    }
}
