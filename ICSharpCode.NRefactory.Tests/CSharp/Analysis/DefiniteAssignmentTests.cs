﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;
using System.Linq;
using NUnit.Framework;

namespace ICSharpCode.NRefactory.CSharp.Analysis
{
	[TestFixture]
	public class DefiniteAssignmentTests
	{
		[Test]
		public void TryFinally()
		{
			BlockStatement block = new BlockStatement {
				new TryCatchStatement {
					TryBlock = new BlockStatement {
						new GotoStatement("LABEL"),
						new AssignmentExpression(new IdentifierExpression("i"), new PrimitiveExpression(1))
					},
					CatchClauses = {
						new CatchClause {
							Body = new BlockStatement {
								new AssignmentExpression(new IdentifierExpression("i"), new PrimitiveExpression(3))
							}
						}
					},
					FinallyBlock = new BlockStatement {
						new AssignmentExpression(new IdentifierExpression("j"), new PrimitiveExpression(5))
					}
				},
				new LabelStatement { Label = "LABEL" },
				new EmptyStatement()
			};
			TryCatchStatement tryCatchStatement = (TryCatchStatement)block.Statements.First();
			Statement stmt1 = tryCatchStatement.TryBlock.Statements.ElementAt(1);
			Statement stmt3 = tryCatchStatement.CatchClauses.Single().Body.Statements.Single();
			Statement stmt5 = tryCatchStatement.FinallyBlock.Statements.Single();
			LabelStatement label = (LabelStatement)block.Statements.ElementAt(1);
			
			DefiniteAssignmentAnalysis da = new DefiniteAssignmentAnalysis(block);
			da.Analyze("i");
			Assert.AreEqual(0, da.UnassignedVariableUses.Count);
			Assert.AreEqual(DefiniteAssignmentStatus.PotentiallyAssigned, da.GetStatusBefore(tryCatchStatement));
			Assert.AreEqual(DefiniteAssignmentStatus.CodeUnreachable, da.GetStatusBefore(stmt1));
			Assert.AreEqual(DefiniteAssignmentStatus.CodeUnreachable, da.GetStatusAfter(stmt1));
			Assert.AreEqual(DefiniteAssignmentStatus.PotentiallyAssigned, da.GetStatusBefore(stmt3));
			Assert.AreEqual(DefiniteAssignmentStatus.DefinitelyAssigned, da.GetStatusAfter(stmt3));
			Assert.AreEqual(DefiniteAssignmentStatus.PotentiallyAssigned, da.GetStatusBefore(stmt5));
			Assert.AreEqual(DefiniteAssignmentStatus.PotentiallyAssigned, da.GetStatusAfter(stmt5));
			Assert.AreEqual(DefiniteAssignmentStatus.DefinitelyAssigned, da.GetStatusAfter(tryCatchStatement));
			Assert.AreEqual(DefiniteAssignmentStatus.DefinitelyAssigned, da.GetStatusBefore(label));
			Assert.AreEqual(DefiniteAssignmentStatus.PotentiallyAssigned, da.GetStatusAfter(label));
			
			da.Analyze("j");
			Assert.AreEqual(0, da.UnassignedVariableUses.Count);
			Assert.AreEqual(DefiniteAssignmentStatus.PotentiallyAssigned, da.GetStatusBefore(tryCatchStatement));
			Assert.AreEqual(DefiniteAssignmentStatus.CodeUnreachable, da.GetStatusBefore(stmt1));
			Assert.AreEqual(DefiniteAssignmentStatus.CodeUnreachable, da.GetStatusAfter(stmt1));
			Assert.AreEqual(DefiniteAssignmentStatus.PotentiallyAssigned, da.GetStatusBefore(stmt3));
			Assert.AreEqual(DefiniteAssignmentStatus.PotentiallyAssigned, da.GetStatusAfter(stmt3));
			Assert.AreEqual(DefiniteAssignmentStatus.PotentiallyAssigned, da.GetStatusBefore(stmt5));
			Assert.AreEqual(DefiniteAssignmentStatus.DefinitelyAssigned, da.GetStatusAfter(stmt5));
			Assert.AreEqual(DefiniteAssignmentStatus.DefinitelyAssigned, da.GetStatusAfter(tryCatchStatement));
			Assert.AreEqual(DefiniteAssignmentStatus.DefinitelyAssigned, da.GetStatusBefore(label));
			Assert.AreEqual(DefiniteAssignmentStatus.DefinitelyAssigned, da.GetStatusAfter(label));
		}
		
		[Test]
		public void ConditionalAnd()
		{
			IfElseStatement ifStmt = new IfElseStatement {
				Condition = new BinaryOperatorExpression {
					Left = new BinaryOperatorExpression(new IdentifierExpression("x"), BinaryOperatorType.GreaterThan, new PrimitiveExpression(0)),
					Operator = BinaryOperatorType.ConditionalAnd,
					Right = new BinaryOperatorExpression {
						Left = new ParenthesizedExpression {
							Expression = new AssignmentExpression {
								Left = new IdentifierExpression("i"),
								Operator = AssignmentOperatorType.Assign,
								Right = new IdentifierExpression("y")
							}
						},
						Operator = BinaryOperatorType.GreaterThanOrEqual,
						Right = new PrimitiveExpression(0)
					}
				},
				TrueStatement = new BlockStatement(),
				FalseStatement = new BlockStatement()
			};
			DefiniteAssignmentAnalysis da = new DefiniteAssignmentAnalysis(ifStmt);
			da.Analyze("i");
			Assert.AreEqual(0, da.UnassignedVariableUses.Count);
			Assert.AreEqual(DefiniteAssignmentStatus.PotentiallyAssigned, da.GetStatusBefore(ifStmt));
			Assert.AreEqual(DefiniteAssignmentStatus.DefinitelyAssigned, da.GetStatusBefore(ifStmt.TrueStatement));
			Assert.AreEqual(DefiniteAssignmentStatus.PotentiallyAssigned, da.GetStatusBefore(ifStmt.FalseStatement));
			Assert.AreEqual(DefiniteAssignmentStatus.PotentiallyAssigned, da.GetStatusAfter(ifStmt));
		}
		
		[Test]
		public void ConditionalOr()
		{
			IfElseStatement ifStmt = new IfElseStatement {
				Condition = new BinaryOperatorExpression {
					Left = new BinaryOperatorExpression(new IdentifierExpression("x"), BinaryOperatorType.GreaterThan, new PrimitiveExpression(0)),
					Operator = BinaryOperatorType.ConditionalOr,
					Right = new BinaryOperatorExpression {
						Left = new ParenthesizedExpression {
							Expression = new AssignmentExpression {
								Left = new IdentifierExpression("i"),
								Operator = AssignmentOperatorType.Assign,
								Right = new IdentifierExpression("y")
							}
						},
						Operator = BinaryOperatorType.GreaterThanOrEqual,
						Right = new PrimitiveExpression(0)
					}
				},
				TrueStatement = new BlockStatement(),
				FalseStatement = new BlockStatement()
			};
			DefiniteAssignmentAnalysis da = new DefiniteAssignmentAnalysis(ifStmt);
			da.Analyze("i");
			Assert.AreEqual(0, da.UnassignedVariableUses.Count);
			Assert.AreEqual(DefiniteAssignmentStatus.PotentiallyAssigned, da.GetStatusBefore(ifStmt));
			Assert.AreEqual(DefiniteAssignmentStatus.PotentiallyAssigned, da.GetStatusBefore(ifStmt.TrueStatement));
			Assert.AreEqual(DefiniteAssignmentStatus.DefinitelyAssigned, da.GetStatusBefore(ifStmt.FalseStatement));
			Assert.AreEqual(DefiniteAssignmentStatus.PotentiallyAssigned, da.GetStatusAfter(ifStmt));
		}
	}
}