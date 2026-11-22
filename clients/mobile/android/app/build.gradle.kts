plugins {
    id("com.android.application")
    // The Flutter Gradle Plugin must be applied after the Android and Kotlin Gradle plugins.
    id("dev.flutter.flutter-gradle-plugin")
}

// Derleme ayarı: önce Gradle özelliği (-P), yoksa ortam değişkeni (CI secret). Depoya secret yazılmaz.
// Ayrıntı: clients/mobile/README.md "Android sürüm derlemesi" ve docs/adr/0008-mobil-istemci-mimarisi.md
fun sgtSetting(name: String): String? =
    (project.findProperty(name) as String?)?.takeIf { it.isNotBlank() }
        ?: System.getenv(name)?.takeIf { it.isNotBlank() }

val isReleaseBuild = gradle.startParameter.taskNames.any { it.contains("Release", ignoreCase = true) }

// --- Sertifika sabitleme (network-security-config) -------------------------------------------------
// Kapsam: Android'in yerel ağ yığını (AppAuth token/yenileme istekleri). Dart HTTP istemcisi ayrıca
// SGT_PINNED_CERT ile sabitlenir. Sistem tarayıcısındaki giriş sayfası bu kapsamın dışındadır.
val nscApiHost = sgtSetting("SGT_API_HOST")
val nscAuthHost = sgtSetting("SGT_AUTH_HOST")
val nscPinPrimary = sgtSetting("SGT_PIN_SHA256_PRIMARY")
val nscPinBackup = sgtSetting("SGT_PIN_SHA256_BACKUP")
val nscPinExpiration = sgtSetting("SGT_PIN_EXPIRATION") // yyyy-MM-dd; isteğe bağlı
val nscCaFile = sgtSetting("SGT_ANDROID_CA_FILE") // casinonun iç CA'sı (PEM); isteğe bağlı
val pinsConfigured = listOf(nscApiHost, nscAuthHost, nscPinPrimary, nscPinBackup).all { it != null }

if (isReleaseBuild && !pinsConfigured) {
    throw GradleException(
        "Sürüm derlemesi sertifika sabitleme ister: SGT_API_HOST, SGT_AUTH_HOST, " +
            "SGT_PIN_SHA256_PRIMARY, SGT_PIN_SHA256_BACKUP verilmeli (README: Android sürüm derlemesi)."
    )
}

val generatedNscRes = layout.buildDirectory.dir("generated/sgt-nsc/res")

val generateNetworkSecurityConfig by tasks.registering {
    inputs.property("hosts", "${nscApiHost.orEmpty()}|${nscAuthHost.orEmpty()}")
    inputs.property("pins", "${nscPinPrimary.orEmpty()}|${nscPinBackup.orEmpty()}|${nscPinExpiration.orEmpty()}")
    inputs.property("ca", nscCaFile.orEmpty())
    outputs.dir(generatedNscRes)
    doLast {
        val res = generatedNscRes.get().asFile
        val xmlDir = File(res, "xml").apply { mkdirs() }
        val anchors = StringBuilder("<certificates src=\"system\" />")
        if (nscCaFile != null) {
            val rawDir = File(res, "raw").apply { mkdirs() }
            File(nscCaFile).copyTo(File(rawDir, "sgt_ca.pem"), overwrite = true)
            anchors.append("<certificates src=\"@raw/sgt_ca\" />")
        }
        val pinned = if (pinsConfigured) {
            val expiration = nscPinExpiration?.let { " expiration=\"$it\"" } ?: ""
            """
            |    <domain-config cleartextTrafficPermitted="false">
            |        <domain includeSubdomains="false">$nscApiHost</domain>
            |        <domain includeSubdomains="false">$nscAuthHost</domain>
            |        <pin-set$expiration>
            |            <pin digest="SHA-256">$nscPinPrimary</pin>
            |            <pin digest="SHA-256">$nscPinBackup</pin>
            |        </pin-set>
            |        <trust-anchors>$anchors</trust-anchors>
            |    </domain-config>
            """.trimMargin()
        } else {
            ""
        }
        File(xmlDir, "network_security_config.xml").writeText(
            """
            |<?xml version="1.0" encoding="utf-8"?>
            |<!-- Üretildi: android/app/build.gradle.kts (generateNetworkSecurityConfig). Elle düzenlenmez. -->
            |<network-security-config>
            |    <base-config cleartextTrafficPermitted="false">
            |        <trust-anchors>$anchors</trust-anchors>
            |    </base-config>
            |$pinned
            |</network-security-config>
            |""".trimMargin()
        )
    }
}

tasks.named("preBuild") { dependsOn(generateNetworkSecurityConfig) }

android {
    namespace = "com.sentezgaming.sgt_mobile"
    compileSdk = flutter.compileSdkVersion
    ndkVersion = flutter.ndkVersion

    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_17
        targetCompatibility = JavaVersion.VERSION_17
    }

    sourceSets {
        getByName("main") { res.srcDir(generatedNscRes) }
    }

    defaultConfig {
        applicationId = "com.sentezgaming.sgt"
        minSdk = flutter.minSdkVersion
        targetSdk = flutter.targetSdkVersion
        versionCode = flutter.versionCode
        versionName = flutter.versionName
        // OIDC dönüş adresi: com.sentezgaming.sgt:/oauthredirect (Keycloak istemcisi sgt-mobile)
        manifestPlaceholders["appAuthRedirectScheme"] = "com.sentezgaming.sgt"
    }

    // Sürüm imzası yalnızca CI secret'larından (ortam değişkeni) okunur; depoda anahtar/parola yoktur.
    // Debug imzası yalnızca debug derlemesinde kullanılır; sürüm derlemesi debug anahtarıyla imzalanmaz.
    val keystore = sgtSetting("SGT_ANDROID_KEYSTORE")
    signingConfigs {
        if (keystore != null) {
            create("release") {
                storeFile = file(keystore)
                storePassword = sgtSetting("SGT_ANDROID_KEYSTORE_PASSWORD")
                keyAlias = sgtSetting("SGT_ANDROID_KEY_ALIAS")
                keyPassword = sgtSetting("SGT_ANDROID_KEY_PASSWORD")
            }
        }
    }

    buildTypes {
        debug {
            signingConfig = signingConfigs.getByName("debug")
        }
        release {
            if (isReleaseBuild && keystore == null) {
                throw GradleException("Sürüm imzası için SGT_ANDROID_KEYSTORE ve parolaları (CI secret) verilmeli.")
            }
            signingConfig = signingConfigs.findByName("release")
        }
    }
}

kotlin {
    compilerOptions {
        jvmTarget = org.jetbrains.kotlin.gradle.dsl.JvmTarget.JVM_17
    }
}

flutter {
    source = "../.."
}
