

/// World Of Data: Copyright 2016-2018 David Joiner

Shader "Unlit/Volumetric"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Volume("Volumetric Texture", 3D) = "white" {}
        _Volume2("Volumetric Texture 2", 3D) = "white" {}

        _opacity("Opacity",float)=0.1
        _emissivity("Emissivity",float)=15
        _albedo("Albedo",float)=0
        _minT("minT",float)=0
        _maxT("maxT",float)=1
        _size("Size",float)=6.0
        _nInt("Num Integration Points",int)=100
        _pointX("Point Source X in Texture Space", float) = 0
        _pointY("Point Source Y", float) = 0
        _pointZ("Point Source Z", float) = 0
        _pointI("Point Source I", float) = 0
        _pointC("Point Source Color", color) = (1,1,1,1)
         

    }
    SubShader
    {
        Tags {             "Queue"="Transparent"
                      "RenderType"="Transparent" }     // make sure the renderer knows to treat this as a transparent object
        LOD 100

        // what about one/two sided? two sided allows you to go "in" the object. Can I reverse the normals and leave as one sided?
        //Cull Off    // one sided or two sided
        Cull Front
        ZWrite Off     // depth testing
        Blend SrcAlpha OneMinusSrcAlpha         // blend with background according to alpha value 


        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;

            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 uv3d : TEXCOORD2;
                float3 cPos : TEXCOORD3;
                float4 pos : SV_POSITION;
            };

            sampler2D _MainTex;
           
            float4 _MainTex_ST;

           

            
            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex); 
                o.worldPos = mul (unity_ObjectToWorld, v.vertex).xyz;  
                o.cPos = mul(unity_ObjectToWorld, half4(0,0,0,1));
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);   // uv space, where are you on textture
                o.uv3d = v.vertex.xyz*0.5+0.5;
                return o;
            }

            sampler3D _Volume;
            sampler3D _Volume2;
   
            float _opacity;
            float _emissivity;
            float _albedo;
            float _size;
            int _nInt;

            float _maxT;
            float _minT;

            float _pointX;
            float _pointY;
            float _pointZ;
            float _pointI;
            float4 _pointC;
 
            float4 positionInObjectSpace(float3 positionInWorldSpace) {
              float3 wpTemp3 = positionInWorldSpace;
                   float4 wpTemp = float4(wpTemp3.x,wpTemp3.y,wpTemp3.z,1);
                   float4 uv3dTemp2 = mul(unity_WorldToObject,wpTemp);
                   return uv3dTemp2;
            }

            float3 positionInTextureSpace(float4 posInObjectSpace) {
                   float3 uv3d = posInObjectSpace.xyz+.5;
                   return uv3d;
            }
             
           // optimize this shader as much as possible. How many extra calculations are being made? What could be done ahead of time?
            fixed4 frag (v2f i) : SV_Target
            {
          
                //ObjectInWorldSpace = float3(_ObX,_ObY, _ObZ);
                //ObjectInWorldSpace = i.cPos;
               
                // determine ray direction from camera
                float3 posRelativeToCamera = i.worldPos-_WorldSpaceCameraPos;
                float3 unitDirectionToCamera = normalize(posRelativeToCamera);// ray points to pixel
                // DONT HARD CODE THIS!!!!
                float size = _size;



                // sample the texture
                int nsteps = _nInt;
                float stepSize = (size/2+min(length(posRelativeToCamera),size/2))/(nsteps+1);
                    // this needs to be optimized, dont step through all of space, just step through size of object, focus main number of steps where
                    // the data is, probe from further out so that things can be seen from main clipping distance
                float4 intensity = float4(0,0,0,0);
                float4 pointSourceIntensity = float4(_pointC.r,_pointC.g,_pointC.b,_pointI);
                posRelativeToCamera += unitDirectionToCamera*stepSize*nsteps/2;  // from object centers frame of reference, far side of object from camera


                //ray trace
                for(int l=0;l<nsteps;l++) {
                   float4 posInObjectSpace = positionInObjectSpace(posRelativeToCamera+_WorldSpaceCameraPos);
                   float3 posInTextureSpace = positionInTextureSpace(posInObjectSpace);

                   // only count ray positions "in the object" for ray trace
                   if(posInObjectSpace.x>-0.5&&posInObjectSpace.x<0.5&&
                       posInObjectSpace.y>-0.5&&posInObjectSpace.y<0.5&&
                       posInObjectSpace.z>-0.5&&posInObjectSpace.z<0.5) {
                       // sample the density map
                       float4 densitySample = tex3D(_Volume,posInTextureSpace); // color in r g b, density in alpha
                       // sample the temperature map
                       float4 temperatureSample = tex3D(_Volume2,posInTextureSpace); // temperature in r channel
                       // get the density from the alpha component of the density map 
                       float columnDensity = stepSize*densitySample.a;
                       // absorb light from the ray based on the opacity
                       intensity *= exp(-_opacity*columnDensity);
                       // emit light at the temperature maps temp at the color given in the density map
                       intensity += _emissivity*columnDensity*densitySample*(_minT+temperatureSample.r*(_maxT-_minT));

                       // add in scattering from point source
                       // find the position of the point source relative to the current world position
                       // trace point source to current world position, determine intensity
                       // of point source at current world position
                       int pointSourceSteps = 10;
                       if(_albedo==0.0 || pointSourceIntensity.a==0.0) {
                       		pointSourceSteps = -1;
                       }
                       float3 pointInLocalSpace = float3(_pointX,_pointY,_pointZ)-(posInTextureSpace);
                       float stepSizeSource = length(pointInLocalSpace)/pointSourceSteps;
                       float3 unitToLocalSpace = -normalize(pointInLocalSpace);
                       float4 currentPointIntensity = pointSourceIntensity*pointSourceIntensity.a;
                       for(int i=0;i<pointSourceSteps;i++) {
                      	 if(pointInLocalSpace.x>0&&pointInLocalSpace.x<1&&
	                       pointInLocalSpace.y>0&&pointInLocalSpace.y<1&&
	                       pointInLocalSpace.z>0&&pointInLocalSpace.z<1) {
							   densitySample = tex3D(_Volume,pointInLocalSpace); // color in r g b, density in alpha
		                       // sample the temperature map
		                       temperatureSample = tex3D(_Volume2,pointInLocalSpace); // temperature in r channel
		                       // get the density from the alpha component of the density map 
		                       columnDensity = stepSizeSource*densitySample.a;
		                       // absorb light from the ray based on the opacity
		                       currentPointIntensity *= exp(-_opacity*columnDensity);
		                       // emit light at the temperature maps temp at the color given in the density map
		                       currentPointIntensity += _emissivity*columnDensity*densitySample*(_minT+temperatureSample.r*(_maxT-_minT));    
	                       }
	                       pointInLocalSpace += unitToLocalSpace*stepSizeSource;
                       } 
                       densitySample = tex3D(_Volume,posInTextureSpace); // color in r g b, density in alpha
                       // sample the temperature map
                       temperatureSample = tex3D(_Volume2,posInTextureSpace); // temperature in r channel
                       // get the density from the alpha component of the density map 
                       columnDensity = stepSize*densitySample.a;
                       intensity += currentPointIntensity*_albedo*columnDensity;  
                   }
                   posRelativeToCamera -= unitDirectionToCamera*stepSize;
                }
              
                // sample the texture
                //fixed4 col = tex2D(_MainTex, i.uv);
                //return col;
                return intensity;
                //return(tex3D(_Volume,i.uv3d));
            }
            ENDCG
        }
    }
}