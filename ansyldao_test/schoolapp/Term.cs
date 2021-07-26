using System;
using ansyl.dao;

namespace ansyldao_test.schoolapp
{
    public class Term : DataObject
    {
        public byte     TermId     { get; set; }
        public byte     TermNo     { get; set; }
        public string   TermName   { get; set; }
        public string   YearName   { get; set; }
        public bool     IsActive   { get; set; }
        public DateTime InsertDate { get; set; }
    }

    public class SchoolTerm : DataObject
    {
        public int       SchoolTermId { get; set; }
        public short     SchoolId     { get; set; }
        public byte      TermId       { get; set; }
        public DateTime? FirstDate    { get; set; }
        public DateTime? FinalDate    { get; set; }
        public bool      IsActive     { get; set; }
        public DateTime  InsertDate   { get; set; }
    }

}