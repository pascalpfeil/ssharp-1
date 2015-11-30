// The MIT License (MIT)
// 
// Copyright (c) 2014-2015, Institute for Software & Systems Engineering
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

namespace SafetySharp.Runtime.Serialization.Serializers
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using Modeling;
	using Reflection;

	/// <summary>
	///   Serializes all kinds of <see cref="Component" />-derived classes marked with <see cref="FaultEffectAttribute" />.
	/// </summary>
	internal sealed class FaultEffectSerializer : ObjectSerializer
	{
		/// <summary>
		///   Checks whether the serialize is able to serialize the <paramref name="obj" />.
		/// </summary>
		/// <param name="obj">The obj that should be checked.</param>
		protected internal override bool CanSerialize(object obj)
		{
			var component = obj as Component;
			return component != null && component.IsFaultEffect();
		}

		/// <summary>
		///   Generates the code to deserialize the <paramref name="obj" />.
		/// </summary>
		/// <param name="generator">The  generator that should be used to generate the code.</param>
		/// <param name="obj">The object that should be deserialized.</param>
		/// <param name="objectIdentifier">The identifier of the <paramref name="obj" />.</param>
		/// <param name="mode">The serialization mode that should be used to deserialize the object.</param>
		protected internal override void Deserialize(SerializationGenerator generator, object obj, int objectIdentifier, SerializationMode mode)
		{
			foreach (var field in GetFields(obj, mode))
				generator.DeserializeField(objectIdentifier, field);
		}

		/// <summary>
		///   Generates the code to serialize the <paramref name="obj" />.
		/// </summary>
		/// <param name="generator">The  generator that should be used to generate the code.</param>
		/// <param name="obj">The object that should be serialized.</param>
		/// <param name="objectIdentifier">The identifier of the <paramref name="obj" />.</param>
		/// <param name="mode">The serialization mode that should be used to serialize the object.</param>
		protected internal override void Serialize(SerializationGenerator generator, object obj, int objectIdentifier, SerializationMode mode)
		{
			foreach (var field in GetFields(obj, mode))
				generator.SerializeField(objectIdentifier, field);
		}

		/// <summary>
		///   Gets the number of state slots required by the serialized data of <paramref name="obj" />.
		/// </summary>
		/// <param name="obj">The object consisting of state values that should be serialized.</param>
		/// <param name="mode">The serialization mode that should be used to serialize the objects.</param>
		protected internal override int GetStateSlotCount(object obj, SerializationMode mode)
		{
			return GetFields(obj, mode).Sum(field => SerializationGenerator.GetStateSlotCount(field.FieldType));
		}

		/// <summary>
		///   Serializes the information about <paramref name="obj" />'s type using the <paramref name="writer" />.
		/// </summary>
		/// <param name="obj">The object whose type information should be serialized.</param>
		/// <param name="writer">The writer the serialized information should be written to.</param>
		protected internal override void SerializeType(object obj, BinaryWriter writer)
		{
			base.SerializeType(obj, writer);

			// ReSharper disable once AssignNullToNotNullAttribute
			writer.Write(((Component)obj).FaultEffectType.AssemblyQualifiedName);
		}

		/// <summary>
		///   Creates an instance of the serialized type stored in the <paramref name="reader" /> without running
		///   any of the type's constructors.
		/// </summary>
		/// <param name="reader">The reader the serialized type information should be read from.</param>
		protected internal override object InstantiateType(BinaryReader reader)
		{
			var obj = (Component)base.InstantiateType(reader);
			obj.FaultEffectType = Type.GetType(reader.ReadString(), throwOnError: true);

			return obj;
		}

		/// <summary>
		///   Gets the fields declared by the <paramref name="obj" /> that should be serialized. In full serialization mode, this only
		///   includes the fields declared by <paramref name="obj" /> itself, not any of the fields declared by its base types. In
		///   optimized mode, this includes all fields. The reason is that in optimized mode, fault effects are actually treated as
		///   components, whereas in full mode, they only serve to serialize the delta to their base class.
		/// </summary>
		/// <param name="obj">The object that should be serialized.</param>
		/// <param name="mode">The serialization mode that should be used to serialize the objects.</param>
		private static IEnumerable<FieldInfo> GetFields(object obj, SerializationMode mode)
		{
			var type = ((Component)obj).FaultEffectType;
			return mode == SerializationMode.Optimized
				? GetFields(obj, mode, null)
				: GetFields(obj, mode, type, type.BaseType);
		}
	}
}