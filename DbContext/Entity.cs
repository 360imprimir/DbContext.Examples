//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DbContextExample
{
    using System;
    using System.Collections.Generic;
    
    public partial class Entity
    {
        public int EntityID { get; set; }
        public Nullable<int> ChildEntityID { get; set; }
        public string Description { get; set; }
    
        public virtual ChildEntity ChildEntity { get; set; }
    }
}
