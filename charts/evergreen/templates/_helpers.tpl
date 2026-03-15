{{/*
Chart name
*/}}
{{- define "evergreen.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Fully qualified app name
*/}}
{{- define "evergreen.fullname" -}}
{{- if .Values.fullnameOverride }}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- $name := default .Chart.Name .Values.nameOverride }}
{{- if contains $name .Release.Name }}
{{- .Release.Name | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- printf "%s-%s" .Release.Name $name | trunc 63 | trimSuffix "-" }}
{{- end }}
{{- end }}
{{- end }}

{{/*
Chart label
*/}}
{{- define "evergreen.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Common labels
*/}}
{{- define "evergreen.labels" -}}
helm.sh/chart: {{ include "evergreen.chart" . }}
{{ include "evergreen.selectorLabels" . }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- end }}

{{/*
Selector labels
*/}}
{{- define "evergreen.selectorLabels" -}}
app.kubernetes.io/name: {{ include "evergreen.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}

{{/*
Image reference helper
*/}}
{{- define "evergreen.image" -}}
{{- $registry := .global.imageRegistry | default "" -}}
{{- if $registry -}}
{{- printf "%s/%s:%s" $registry .image.repository (.image.tag | default "latest") -}}
{{- else -}}
{{- printf "%s:%s" .image.repository (.image.tag | default "latest") -}}
{{- end -}}
{{- end }}

{{/*
Database environment variables
*/}}
{{- define "evergreen.dbEnv" -}}
- name: POSTGRES_USER
  valueFrom:
    secretKeyRef:
      name: {{ include "evergreen.fullname" . }}-db
      key: username
- name: POSTGRES_PASSWORD
  valueFrom:
    secretKeyRef:
      name: {{ include "evergreen.fullname" . }}-db
      key: password
- name: POSTGRES_DB
  valueFrom:
    secretKeyRef:
      name: {{ include "evergreen.fullname" . }}-db
      key: database
- name: POSTGRES_HOST
  value: {{ include "evergreen.fullname" . }}-db
- name: POSTGRES_PORT
  value: "5432"
{{- end }}

{{/*
Wait-for-ejabberd init container
*/}}
{{- define "evergreen.waitForEjabberd" -}}
- name: wait-for-ejabberd
  image: busybox:1.36
  command: ['sh', '-c', 'until nc -z private-ejabberd 5222; do echo "waiting for ejabberd..."; sleep 2; done']
{{- end }}

{{/*
Wait-for-db init container
*/}}
{{- define "evergreen.waitForDb" -}}
- name: wait-for-db
  image: busybox:1.36
  command: ['sh', '-c', 'until nc -z {{ include "evergreen.fullname" . }}-db 5432; do echo "waiting for db..."; sleep 2; done']
{{- end }}
