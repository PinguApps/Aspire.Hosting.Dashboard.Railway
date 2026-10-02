# Deployment behaviour

Pre-create and record the Railway project/environment. Set its shared `PINGUAPPS_SITE_KEY` marker to the target site key. Supply a production-environment project token only through the secret target parameter.

Run `aspire deploy`. The shared provider verifies target scope and ownership, then reconciles a single immutable-image dashboard service. Repeated unchanged deployments reuse service/domain identities and the current successful deployment. Unproven ownership and cached identity drift fail before writes. Services and volumes are never automatically deleted.

The dashboard is always awake, uses one replica and no persistent volume. Its telemetry is temporary. Restart/redeployment can remove diagnostics; durable central observation belongs to the separately managed shared OpenObserve service.

Only frontend 18888 is published. No public TCP proxy or ingestion domain is configured. There is no deployed AppHost resource service, so the viewer does not manage production resources.
