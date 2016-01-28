﻿using System;
using System.Collections.Generic;
using Neo4j.Driver.Exceptions;
using Neo4j.Driver.Extensions;

namespace Neo4j.Driver.Internal.result
{
    internal class SummaryBuilder
    {
        public Statement Statement { private get; set; }
        public StatementType StatementType { private get; set; }
        public IUpdateStatistics UpdateStatistics { private get; set; }
        public IPlan Plan { private get; set; }
        public IProfiledPlan Profile { private get; set; }
        public IList<INotification> Notifications { private get; set; }

        public SummaryBuilder(Statement statement)
        {
            Statement = statement;
        }

        public IResultSummary Build()
        {
            return new ResultSumamry(this);
        }

        private class ResultSumamry:IResultSummary
        {
            public ResultSumamry(SummaryBuilder builder)
            {
                Throw.ArgumentNullException.IfNull(builder.Statement, nameof(builder.Statement));
                //Throw.ArgumentNullException.IfNull(builder.StatementType, nameof(builder.StatementType));
                Statement = builder.Statement;
                StatementType = builder.StatementType;
                UpdateStatistics = builder.UpdateStatistics ?? new UpdateStatistics();
                Plan = builder.Plan;
                Profile = builder.Profile;
                Notifications = builder.Notifications ?? new List<INotification>();
            }

            public Statement Statement { get; }
            public IUpdateStatistics UpdateStatistics { get; }
            public StatementType StatementType { get; }
            public bool HasPlan => Plan != null;
            public bool HasProfile => Profile != null;
            public IPlan Plan { get; }
            public IProfiledPlan Profile { get; }
            public IList<INotification> Notifications { get; }

            public override string ToString()
            {
                return $"{GetType().Name}{{{nameof(Statement)}={Statement}, " +
                       $"{nameof(UpdateStatistics)}={UpdateStatistics}, " +
                       $"{nameof(StatementType)}={StatementType}, " +
                       $"{nameof(Plan)}={Plan}, " +
                       $"{nameof(Profile)}={Profile}, " +
                       $"{nameof(Notifications)}={Notifications.ToContentString()}}}";
            }
        }
    }

    public class Plan : IPlan
    {
        public Plan(string operationType, Dictionary<string, object> args, IList<string> identifiers, List<IPlan> childPlans)
        {
            OperatorType = operationType;
            Arguments = args;
            Identifiers = identifiers;
            Children = childPlans;
        }

        public string OperatorType { get; }
        public IDictionary<string, object> Arguments { get; }
        public IList<string> Identifiers { get; }
        public IList<IPlan> Children { get; }

        public override string ToString()
        {
            return $"{GetType().Name}{{{nameof(OperatorType)}={OperatorType}, " +
                   $"{nameof(Arguments)}={Arguments.ToContentString()}, " +
                   $"{nameof(Identifiers)}={Identifiers.ToContentString()}, " +
                   $"{nameof(Children)}={Children.ToContentString()}}}";
        }
    }

    public class ProfiledPlan : IProfiledPlan
    {
        public ProfiledPlan(string operatorType, IDictionary<string, object> arguments, IList<string> identifiers, IList<IProfiledPlan> children, long dbHits, long records)
        {
            OperatorType = operatorType;
            Arguments = arguments;
            Identifiers = identifiers;
            Children = children;
            DbHits = dbHits;
            Records = records;
        }

        public string OperatorType { get; }

        public IDictionary<string, object> Arguments { get; }

        public IList<string> Identifiers { get; }

        IList<IPlan> IPlan.Children { get { throw new InvalidOperationException("This is a profiled plan.");} }

        public IList<IProfiledPlan> Children { get; }

        public long DbHits { get; }

        public long Records { get; }

        public override string ToString()
        {
            return $"{GetType().Name}{{{nameof(OperatorType)}={OperatorType}, " +
                   $"{nameof(Arguments)}={Arguments.ToContentString()}, " +
                   $"{nameof(Identifiers)}={Identifiers.ToContentString()}, " +
                   $"{nameof(DbHits)}={DbHits}, " +
                   $"{nameof(Records)}={Records}, " +
                   $"{nameof(Children)}={Children.ToContentString()}}}";
        }
    }

    public class UpdateStatistics : IUpdateStatistics
    {

        public bool ContainsUpdates => (
            IsPositive(NodesCreated)
            || IsPositive(NodesDeleted)
            || IsPositive(RelationshipsCreated)
            || IsPositive(RelationshipsDeleted)
            || IsPositive(PropertiesSet)
            || IsPositive(LabelsAdded)
            || IsPositive(LabelsRemoved)
            || IsPositive(IndexesAdded)
            || IsPositive(IndexesRemoved)
            || IsPositive(ConstraintsAdded)
            || IsPositive(ConstraintsRemoved));
        public int NodesCreated { get; }
        public int NodesDeleted { get; }
        public int RelationshipsCreated { get; }
        public int RelationshipsDeleted { get; }
        public int PropertiesSet { get; }
        public int LabelsAdded { get; }
        public int LabelsRemoved { get; }
        public int IndexesAdded { get; }
        public int IndexesRemoved { get; }
        public int ConstraintsAdded { get; }
        public int ConstraintsRemoved { get; }

        public UpdateStatistics():this(0,0,0,0,0,0,0,0,0,0,0)
        { }

        public UpdateStatistics(int nodesCreated, int nodesDeleted, int relationshipsCreated, int relationshipsDeleted, int propertiesSet, int labelsAdded, int labelsRemoved, int indexesAdded, int indexesRemoved, int constraintsAdded, int constraintsRemoved)
        {
            NodesCreated = nodesCreated;
            NodesDeleted = nodesDeleted;
            RelationshipsCreated = relationshipsCreated;
            RelationshipsDeleted = relationshipsDeleted;
            PropertiesSet = propertiesSet;
            LabelsAdded = labelsAdded;
            LabelsRemoved = labelsRemoved;
            IndexesAdded = indexesAdded;
            IndexesRemoved = indexesRemoved;
            ConstraintsAdded = constraintsAdded;
            ConstraintsRemoved = constraintsRemoved;
        }

        private bool IsPositive(int value)
        {
            return value > 0;
        }
        public override string ToString()
        {
            return $"{GetType().Name}{{{nameof(NodesCreated)}={NodesCreated}, " +
                   $"{nameof(NodesDeleted)}={NodesDeleted}, " +
                   $"{nameof(RelationshipsCreated)}={RelationshipsCreated}, " +
                   $"{nameof(RelationshipsDeleted)}={RelationshipsDeleted}, " +
                   $"{nameof(PropertiesSet)}={PropertiesSet}, " +
                   $"{nameof(LabelsAdded)}={LabelsAdded}, " +
                   $"{nameof(LabelsRemoved)}={LabelsRemoved}, " +
                   $"{nameof(IndexesAdded)}={IndexesAdded}, " +
                   $"{nameof(IndexesRemoved)}={IndexesRemoved}, " +
                   $"{nameof(ConstraintsAdded)}={ConstraintsAdded}, " +
                   $"{nameof(ConstraintsRemoved)}={ConstraintsRemoved}}}";
        }

    }

    /// <summary>
    /// This is a notifcation
    /// </summary>
    public class Notification : INotification
    {
        public string Code { get; }
        public string Title { get; }
        public string Description { get; }
        public IInputPosition Position { get; }

        public Notification(string code, string title, string description, IInputPosition position)
        {
            Code = code;
            Title = title;
            Description = description;
            Position = position;
        }

        public override string ToString()
        {
            return $"{GetType().Name}{{{nameof(Code)}={Code}, " +
                   $"{nameof(Title)}={Title}, " +
                   $"{nameof(Description)}={Description}, " +
                   $"{nameof(Position)}={Position}}}";
        }

    }

    public class InputPosition : IInputPosition
    {
        public int Offset { get; }
        public int Line { get; }
        public int Column { get; }

        public InputPosition(int offset, int line, int column)
        {
            Offset = offset;
            Line = line;
            Column = column;
        }

        public override string ToString()
        {
            return $"{GetType().Name}{{{nameof(Offset)}={Offset}, " +
                   $"{nameof(Line)}={Line}, " +
                   $"{nameof(Column)}={Column}}}";
        }
    }
}