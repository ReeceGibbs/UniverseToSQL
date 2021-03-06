//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AutozoneSyncDataAccess
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class Entities : DbContext
    {
        public Entities()
            : base("name=Entities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
    
        public virtual int BulkInsert(string destinationTable, string sourcePath, string rowNum)
        {
            var destinationTableParameter = destinationTable != null ?
                new ObjectParameter("destinationTable", destinationTable) :
                new ObjectParameter("destinationTable", typeof(string));
    
            var sourcePathParameter = sourcePath != null ?
                new ObjectParameter("sourcePath", sourcePath) :
                new ObjectParameter("sourcePath", typeof(string));
    
            var rowNumParameter = rowNum != null ?
                new ObjectParameter("rowNum", rowNum) :
                new ObjectParameter("rowNum", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction("BulkInsert", destinationTableParameter, sourcePathParameter, rowNumParameter);
        }
    }
}
