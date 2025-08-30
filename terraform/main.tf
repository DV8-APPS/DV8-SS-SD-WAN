module "ssfm" {
  source = "./modules/ssfm"
  cluster_name = "dv8-sovereign"
  namespace    = "ssfm"
  enable_discovery = true
  enable_ccp       = true
  enable_quantumshield = true
  hoac_default_profile = "ECC"
  hoac_pqc_threshold   = 70
  aic_capsules         = ["SessionOracle","EntropyGuard","KeySurgeon","VectorMind"]
}
