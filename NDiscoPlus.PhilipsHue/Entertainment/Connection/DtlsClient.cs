using Org.BouncyCastle.Crypto.Tls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NDiscoPlus.PhilipsHue.Entertainment.Connection;

// Yoinked from: https://github.com/michielpost/Q42.HueApi/blob/e3d059128e11ee19a7cac9fd2265bee989ac4271/src/HueApi.Entertainment/Connection/DtlsClient.cs
// Fixed a small issue with deprecated constructor
internal class DtlsClient : DefaultTlsClient
{
    // private readonly TlsSession? _mSession;
    private readonly TlsPskIdentity _mPskIdentity;

    internal DtlsClient(TlsSession? session, TlsPskIdentity pskIdentity)
    {
        // _mSession = session;
        _mPskIdentity = pskIdentity;
    }

    public override ProtocolVersion MinimumVersion
    {
        get { return ProtocolVersion.DTLSv12; }
    }

    public override ProtocolVersion ClientVersion
    {
        get { return ProtocolVersion.DTLSv12; }
    }

    public override int[] GetCipherSuites()
    {
        //Req Cipher for Philips Hue
        return new int[] { CipherSuite.TLS_PSK_WITH_AES_128_GCM_SHA256 };
    }


    public override IDictionary GetClientExtensions()
    {
        IDictionary clientExtensions = TlsExtensionsUtilities.EnsureExtensionsInitialised(base.GetClientExtensions());
        TlsExtensionsUtilities.AddEncryptThenMacExtension(clientExtensions);
        TlsExtensionsUtilities.AddExtendedMasterSecretExtension(clientExtensions);

        return clientExtensions;
    }
    public override TlsAuthentication GetAuthentication()
    {
        return new MyTlsAuthentication(mContext);
    }

    public override TlsKeyExchange GetKeyExchange()
    {
        int keyExchangeAlgorithm = TlsUtilities.GetKeyExchangeAlgorithm(mSelectedCipherSuite);

        switch (keyExchangeAlgorithm)
        {
            case KeyExchangeAlgorithm.DHE_PSK:
            case KeyExchangeAlgorithm.ECDHE_PSK:
            case KeyExchangeAlgorithm.PSK:
            case KeyExchangeAlgorithm.RSA_PSK:
                return CreatePskKeyExchange(keyExchangeAlgorithm);

            case KeyExchangeAlgorithm.ECDH_anon:
            case KeyExchangeAlgorithm.ECDH_ECDSA:
            case KeyExchangeAlgorithm.ECDH_RSA:
                return CreateECDHKeyExchange(keyExchangeAlgorithm);

            default:
                /*
                    * Note: internal error here; the TlsProtocol implementation verifies that the
                    * server-selected cipher suite was in the list of client-offered cipher suites, so if
                    * we now can't produce an implementation, we shouldn't have offered it!
                    */
                throw new TlsFatalAlert(AlertDescription.internal_error);
        }
    }

    internal class MyTlsAuthentication
        : TlsAuthentication
    {
        private readonly TlsContext _mContext;

        internal MyTlsAuthentication(TlsContext context)
        {
            _mContext = context;
        }

        public virtual void NotifyServerCertificate(Certificate serverCertificate)
        {

        }

        public virtual TlsCredentials? GetClientCredentials(CertificateRequest certificateRequest)
        {
            return null;
        }

    };

    protected virtual TlsKeyExchange CreatePskKeyExchange(int keyExchange)
    {
        return new TlsPskKeyExchange(
            keyExchange,
            mSupportedSignatureAlgorithms,
            _mPskIdentity,
            null,
            mDHVerifier,
            null,
            mNamedCurves,
            mClientECPointFormats,
            mServerECPointFormats
        );
    }

    protected override TlsKeyExchange CreateECDHKeyExchange(int keyExchange)
    {
        return new TlsECDHKeyExchange(
            keyExchange,
            mSupportedSignatureAlgorithms,
            mNamedCurves,
            mClientECPointFormats,
            mServerECPointFormats
        );
    }

}