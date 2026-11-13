import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation } from '@tanstack/react-query';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import { Input } from '@/shared/components/ui/Input';
import { Textarea } from '@/shared/components/ui/Textarea';
import { Select } from '@/shared/components/ui/Select';
import { PasswordInput } from '@/shared/components/ui/PasswordInput';
import { Checkbox } from '@/shared/components/ui/Checkbox';
import { Button } from '@/shared/components/ui/Button';
import { RoleToggle } from '@/features/auth/components/RoleToggle';
import { signupDoctor, signupPatient } from '@/features/auth/api/authApi';
import { useAuthStore } from '@/features/auth/store/authStore';
import type { SignupRole } from '@/features/auth/types';
import {
  doctorSignupSchema,
  patientSignupSchema,
  type DoctorSignupFormValues,
  type PatientSignupFormValues,
} from '@/features/auth/schemas/authSchemas';

/** Small section heading used between groups of fields in both variants below. */
function SectionHeading({ children }: { children: string }) {
  return (
    <p className="mb-3 mt-5 text-xs font-bold uppercase tracking-wide text-text-muted first:mt-0">
      {children}
    </p>
  );
}

function SignupErrorBanner({ show }: { show: boolean }) {
  const { t } = useTranslation();
  if (!show) return null;
  return (
    <p className="mb-4 rounded-lg bg-danger/10 px-3 py-2 text-xs text-danger">
      {t('auth.errors.signupFailed')}
    </p>
  );
}

function PatientSignupFields() {
  const { t, i18n } = useTranslation();
  const navigate = useNavigate();
  const setSession = useAuthStore((state) => state.setSession);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<PatientSignupFormValues>({ resolver: zodResolver(patientSignupSchema) });

  const mutation = useMutation({
    mutationFn: (values: PatientSignupFormValues) =>
      signupPatient({
        fullName: values.fullName,
        phone: values.phone,
        email: values.email,
        password: values.password,
        age: values.age,
        gender: values.gender,
        nationalId: values.nationalId,
        diagnosis: values.diagnosis,
        medications: values.medications || undefined,
        assignedDoctorName: values.assignedDoctorName || undefined,
        deviceSerial: values.deviceSerial || undefined,
        preferredLanguage: i18n.language === 'ar' ? 'ar' : 'en',
      }),
    onSuccess: (response) => {
      setSession(response.accessToken, response.user);
      navigate('/patient', { replace: true });
    },
  });

  const onSubmit = handleSubmit((values) => mutation.mutate(values));
  const err = (field: keyof PatientSignupFormValues) =>
    errors[field] && t(errors[field]?.message ?? 'auth.errors.required');

  return (
    <form onSubmit={onSubmit} noValidate>
      <SectionHeading>{t('auth.sectionPersonal')}</SectionHeading>
      <Input
        label={t('auth.fields.fullName')}
        placeholder={t('auth.fields.fullNamePh')}
        required
        error={err('fullName')}
        {...register('fullName')}
      />
      <div className="grid grid-cols-2 gap-3">
        <Input
          label={t('auth.fields.phone')}
          placeholder={t('auth.fields.phonePh')}
          required
          error={err('phone')}
          {...register('phone')}
        />
        <Input
          label={t('auth.fields.age')}
          type="number"
          placeholder={t('auth.fields.agePh')}
          required
          error={err('age')}
          {...register('age')}
        />
      </div>
      <div className="grid grid-cols-2 gap-3">
        <Select
          label={t('auth.fields.gender')}
          placeholder={t('auth.fields.genderSelect')}
          required
          error={err('gender')}
          options={[
            { value: 'male', label: t('auth.fields.genderMale') },
            { value: 'female', label: t('auth.fields.genderFemale') },
          ]}
          {...register('gender')}
        />
        <Input
          label={t('auth.fields.nationalId')}
          placeholder={t('auth.fields.nationalIdPh')}
          required
          inputMode="numeric"
          error={err('nationalId')}
          {...register('nationalId')}
        />
      </div>
      <Input
        label={t('auth.fields.email')}
        type="email"
        placeholder={t('auth.fields.emailPh')}
        required
        error={err('email')}
        {...register('email')}
      />

      <SectionHeading>{t('auth.sectionPatientMedical')}</SectionHeading>
      <Textarea
        label={t('auth.fields.diagnosis')}
        placeholder={t('auth.fields.diagnosisPh')}
        required
        error={err('diagnosis')}
        {...register('diagnosis')}
      />
      <Textarea
        label={t('auth.fields.medications')}
        placeholder={t('auth.fields.medicationsPh')}
        {...register('medications')}
      />
      <Input
        label={t('auth.fields.assignedDoctor')}
        placeholder={t('auth.fields.assignedDoctorPh')}
        helpText={t('auth.fields.assignedDoctorHelp')}
        {...register('assignedDoctorName')}
      />
      <Input
        label={t('auth.fields.deviceSerial')}
        placeholder={t('auth.fields.deviceSerialPh')}
        helpText={t('auth.fields.deviceSerialHelp')}
        {...register('deviceSerial')}
      />

      <SectionHeading>{t('auth.sectionSecurity')}</SectionHeading>
      <PasswordInput
        label={t('auth.fields.password')}
        placeholder={t('auth.fields.passwordPh')}
        required
        error={err('password')}
        {...register('password')}
      />
      <PasswordInput
        label={t('auth.fields.confirmPassword')}
        placeholder={t('auth.fields.confirmPasswordPh')}
        required
        error={err('confirmPassword')}
        {...register('confirmPassword')}
      />
      <Checkbox
        label={t('auth.consent')}
        error={err('consent')}
        {...register('consent')}
      />

      <SignupErrorBanner show={mutation.isError} />

      <Button type="submit" size="lg" isLoading={mutation.isPending} className="mb-3">
        {t('auth.submitSignup')}
      </Button>
      <p className="text-center text-xs text-text-muted">{t('auth.prototypeNoteSignup')}</p>
    </form>
  );
}

function DoctorSignupFields() {
  const { t, i18n } = useTranslation();
  const navigate = useNavigate();
  const setSession = useAuthStore((state) => state.setSession);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<DoctorSignupFormValues>({ resolver: zodResolver(doctorSignupSchema) });

  const mutation = useMutation({
    mutationFn: (values: DoctorSignupFormValues) =>
      signupDoctor({
        fullName: values.fullName,
        phone: values.phone,
        email: values.email,
        password: values.password,
        specialty: values.specialty,
        licenseNumber: values.licenseNumber,
        hospital: values.hospital,
        department: values.department || undefined,
        preferredLanguage: i18n.language === 'ar' ? 'ar' : 'en',
      }),
    onSuccess: (response) => {
      setSession(response.accessToken, response.user);
      navigate('/doctor/patients', { replace: true });
    },
  });

  const onSubmit = handleSubmit((values) => mutation.mutate(values));
  const err = (field: keyof DoctorSignupFormValues) =>
    errors[field] && t(errors[field]?.message ?? 'auth.errors.required');

  return (
    <form onSubmit={onSubmit} noValidate>
      <SectionHeading>{t('auth.sectionPersonal')}</SectionHeading>
      <Input
        label={t('auth.fields.fullName')}
        placeholder={t('auth.fields.fullNamePh')}
        required
        error={err('fullName')}
        {...register('fullName')}
      />
      <div className="grid grid-cols-2 gap-3">
        <Input
          label={t('auth.fields.phone')}
          placeholder={t('auth.fields.phonePh')}
          required
          error={err('phone')}
          {...register('phone')}
        />
        <Input
          label={t('auth.fields.email')}
          type="email"
          placeholder={t('auth.fields.emailPh')}
          required
          error={err('email')}
          {...register('email')}
        />
      </div>

      <SectionHeading>{t('auth.sectionDoctorProfessional')}</SectionHeading>
      <Input
        label={t('auth.fields.specialty')}
        placeholder={t('auth.fields.specialtyPh')}
        required
        error={err('specialty')}
        {...register('specialty')}
      />
      <div className="grid grid-cols-2 gap-3">
        <Input
          label={t('auth.fields.license')}
          placeholder={t('auth.fields.licensePh')}
          required
          error={err('licenseNumber')}
          {...register('licenseNumber')}
        />
        <Input
          label={t('auth.fields.department')}
          placeholder={t('auth.fields.departmentPh')}
          {...register('department')}
        />
      </div>
      <Input
        label={t('auth.fields.hospital')}
        placeholder={t('auth.fields.hospitalPh')}
        required
        error={err('hospital')}
        {...register('hospital')}
      />

      <SectionHeading>{t('auth.sectionSecurity')}</SectionHeading>
      <PasswordInput
        label={t('auth.fields.password')}
        placeholder={t('auth.fields.passwordPh')}
        required
        error={err('password')}
        {...register('password')}
      />
      <PasswordInput
        label={t('auth.fields.confirmPassword')}
        placeholder={t('auth.fields.confirmPasswordPh')}
        required
        error={err('confirmPassword')}
        {...register('confirmPassword')}
      />
      <Checkbox
        label={t('auth.consent')}
        error={err('consent')}
        {...register('consent')}
      />

      <SignupErrorBanner show={mutation.isError} />

      <Button type="submit" size="lg" isLoading={mutation.isPending} className="mb-3">
        {t('auth.submitSignup')}
      </Button>
      <p className="text-center text-xs text-text-muted">{t('auth.prototypeNoteSignup')}</p>
    </form>
  );
}

export function SignupForm() {
  const [role, setRole] = useState<SignupRole>('patient');

  return (
    <div>
      <RoleToggle value={role} onChange={setRole} />
      {role === 'patient' ? <PatientSignupFields /> : <DoctorSignupFields />}
    </div>
  );
}
