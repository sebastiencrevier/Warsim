// IOAuthSettings.cs
// ------------------------------------------------------------------
//
// An interface for OAuth settings, for use with Cropper plugins
// that use OAuth.
//
// ------------------------------------------------------------------
//
// Copyright (c) 2011 by Dino Chiesa
// All rights reserved!
//
// ------------------------------------------------------------------

using System;

namespace Warsim.Core.Helpers.OAuth
{
    interface IOAuthSettings
    {
        string AccessToken
        {
            get;set;
        }
        string AccessSecret
        {
            get;set;
        }
    }
}
