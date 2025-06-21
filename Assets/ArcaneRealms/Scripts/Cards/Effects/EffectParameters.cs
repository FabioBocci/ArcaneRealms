using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace ArcaneRealms.Scripts.Cards.Effects {

	[Serializable]
	public class EffectParameters : INetworkSerializable, IEnumerable<Parameter> {

		[SerializeField] private Parameter[] parametersArray;

		public EffectParameters() {
			parametersArray = new Parameter[0];
		}

		public EffectParameters(List<Parameter> parametersList) {
			parametersArray = parametersList.ToArray();
		}

		public EffectParameters(Parameter[] parameters) {
			parametersArray = parameters;
		}

		public int GetSize() { return parametersArray.Length; }

		public Parameter this[int index] {
			get { return GetParameters()[index]; }
			set { GetParameters()[index] = value; }
		}

		public void Add(Parameter parameter) {
			List<Parameter> parameterlist = new(parametersArray) {
				parameter
			};
			parametersArray = parameterlist.ToArray();
		}

		public EffectParameters AddAll(EffectParameters effectParam) {
			Array.Resize(ref parametersArray, parametersArray.Length + effectParam.parametersArray.Length);
			int j = 0;
			for(int i = parametersArray.Length - effectParam.parametersArray.Length; i <= parametersArray.Length - 1; i++) {
				parametersArray[i] = effectParam.parametersArray[j++];
			}
			return this;
		}


		public IEnumerator<Parameter> GetEnumerator() {
			return new List<Parameter>(parametersArray).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		public string GetParameterStringValue(string key) {
			Parameter parameter = Array.Find(parametersArray, p => p.Key == key);
			return parameter.Value;

		}

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
			serializer.SerializeValue(ref parametersArray);
		}

		private Parameter[] GetParameters() {
			return parametersArray;
		}

	}

	[Serializable]
	public class Parameter : INetworkSerializable {
		public string Key;
		public string Value;
		public string Type;

		public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
			serializer.SerializeValue(ref Key);
			serializer.SerializeValue(ref Value);
			serializer.SerializeValue(ref Type);
		}
	}
}