//
// CallbackHelpers.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2015 Xamarin, Inc.
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

#if SECURITY_DEP

#if MONO_X509_ALIAS
extern alias PrebuiltSystem;
#endif
#if MONO_SECURITY_ALIAS
extern alias MonoSecurity;
#endif

#if MONO_SECURITY_ALIAS
using MSI = MonoSecurity::Mono.Security.Interface;
#else
using MSI = Mono.Security.Interface;
#endif
#if MONO_X509_ALIAS
using XX509CertificateCollection = PrebuiltSystem::System.Security.Cryptography.X509Certificates.X509CertificateCollection;
using XX509Chain = PrebuiltSystem::System.Security.Cryptography.X509Certificates.X509Chain;
#else
using XX509CertificateCollection = System.Security.Cryptography.X509Certificates.X509CertificateCollection;
using XX509Chain = System.Security.Cryptography.X509Certificates.X509Chain;
#endif

using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Mono.Net.Security.Private
{
	/*
	 * Strictly private - do not use outside the Mono.Net.Security directory.
	 */
	static class CallbackHelpers
	{
		internal static MSI.MonoRemoteCertificateValidationCallback PublicToMono (RemoteCertificateValidationCallback callback)
		{
			if (callback == null)
				return null;

			return (h, c, ch, e) => callback (h, c, (X509Chain)(object)ch, (SslPolicyErrors)e);
		}

		internal static MSI.MonoLocalCertificateSelectionCallback PublicToMono (LocalCertificateSelectionCallback callback)
		{
			if (callback == null)
				return null;

			return (t, lc, rc, ai) => callback (null, t, (XX509CertificateCollection)(object)lc, rc, ai);
		}

		internal static MSI.MonoRemoteCertificateValidationCallback InternalToMono (RemoteCertValidationCallback callback)
		{
			if (callback == null)
				return null;

			return (h, c, ch, e) => callback (h, c, (X509Chain)(object)ch, (SslPolicyErrors)e);
		}

		internal static RemoteCertificateValidationCallback InternalToPublic (string hostname, RemoteCertValidationCallback callback)
		{
			if (callback == null)
				return null;

			return (s, c, ch, e) => callback (hostname, c, ch, e);
		}

		internal static MSI.MonoLocalCertificateSelectionCallback InternalToMono (LocalCertSelectionCallback callback)
		{
			if (callback == null)
				return null;

			return (t, lc, rc, ai) => callback (t, (XX509CertificateCollection)(object)lc, rc, ai);
		}

		internal static RemoteCertificateValidationCallback MonoToPublic (MSI.MonoRemoteCertificateValidationCallback callback)
		{
			if (callback == null)
				return null;

			return (t, c, ch, e) => callback (null, c, (XX509Chain)(object)ch, (MSI.MonoSslPolicyErrors)e);
		}

		internal static LocalCertificateSelectionCallback MonoToPublic (MSI.MonoLocalCertificateSelectionCallback callback)
		{
			if (callback == null)
				return null;

			return (s, t, lc, rc, ai) => callback (t, (XX509CertificateCollection)(object)lc, rc, ai);
		}

		internal static RemoteCertValidationCallback MonoToInternal (MSI.MonoRemoteCertificateValidationCallback callback)
		{
			if (callback == null)
				return null;

			return (h, c, ch, e) => callback (h, c, (XX509Chain)(object)ch, (MSI.MonoSslPolicyErrors)e);
		}

		internal static LocalCertSelectionCallback MonoToInternal (MSI.MonoLocalCertificateSelectionCallback callback)
		{
			if (callback == null)
				return null;

			return (t, lc, rc, ai) => callback (t, (XX509CertificateCollection)(object)lc, rc, ai);
		}

#if INSIDE_SYSTEM
		internal static ChainValidationHelper CreateInternalValidationHelper (
			string hostname, bool checkCertName, bool checkCertRevocationStatus,
			RemoteCertValidationCallback remoteValidationCallback,
			LocalCertSelectionCallback certSelectionDelegate)
		{
			var helper = new ChainValidationHelper (InternalToPublic (hostname, remoteValidationCallback));
			helper.LocalCertSelectionCallback = certSelectionDelegate;
			helper.CheckCertificateName = checkCertName;
			helper.CheckCertificateRevocationStatus = checkCertRevocationStatus;
			return helper;
		}

		internal static ChainValidationHelper GetInternalValidationHelper (MSI.CertificateValidationHelper validationHelper)
		{
			if (validationHelper == null)
				return null;
			return new ChainValidationHelper (validationHelper);
		}
#endif

		internal static MSI.CertificateValidationHelper GetPublicValidationHelper (ChainValidationHelper validationHelper)
		{
			if (validationHelper == null)
				return null;
			return validationHelper.GetPublicHelper ();
		}

		internal static MSI.CertificateValidationHelper CreatePublicValidationHelper (
			bool checkCertName, bool checkCertRevocationStatus,
			RemoteCertValidationCallback remoteValidationCallback,
			LocalCertSelectionCallback certSelectionDelegate)
		{
			var helper = new MSI.CertificateValidationHelper ();
			if (remoteValidationCallback != null)
				helper.ServerCertificateValidationCallback = InternalToMono (remoteValidationCallback);
			if (certSelectionDelegate != null)
				helper.ClientCertificateSelectionCallback = InternalToMono (certSelectionDelegate);
			helper.CheckCertificateName = checkCertName;
			helper.CheckCertificateRevocationStatus = checkCertRevocationStatus;
			return helper;
		}
	}
}

#endif
